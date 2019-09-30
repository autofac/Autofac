using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Builder;
using Autofac.Features.Decorators;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Keeps track of the status of registered services.
    /// </summary>
    internal class DefaultRegisteredServicesTracker : Disposable, IRegisteredServicesTracker
    {
        /// <summary>
        /// Keeps track of the status of registered services.
        /// </summary>
        private readonly ConcurrentDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new ConcurrentDictionary<Service, ServiceRegistrationInfo>();

        /// <summary>
        /// External registration sources.
        /// </summary>
        private readonly List<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// All registrations.
        /// </summary>
        private readonly List<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        private readonly ConcurrentDictionary<IServiceWithType, IReadOnlyList<IComponentRegistration>> _decorators
            = new ConcurrentDictionary<IServiceWithType, IReadOnlyList<IComponentRegistration>>();

        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        private readonly object _synchRoot = new object();

        /// <inheritdoc />
        public IEnumerable<IComponentRegistration> Registrations
        {
            get
            {
                lock (_synchRoot)
                    return _registrations.ToArray();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRegistrationSource> Sources
        {
            get
            {
                lock (_synchRoot)
                {
                    return _dynamicRegistrationSources.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public virtual void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
        {
            foreach (var service in registration.Services)
            {
                var info = GetServiceInfo(service);
                info.AddImplementation(registration, preserveDefaults, originatedFromSource);
            }

            _registrations.Add(registration);
            UpdateInitializedAdapters(registration);
        }

        /// <inheritdoc />
        public void AddRegistrationSource(IRegistrationSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            lock (_synchRoot)
            {
                _dynamicRegistrationSources.Insert(0, source);
                foreach (var serviceRegistrationInfo in _serviceInfo)
                    serviceRegistrationInfo.Value.Include(source);
            }
        }

        /// <inheritdoc />
        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var info = GetInitializedServiceInfoOrDefault(service);
            if (info != null && info.TryGetRegistration(out registration))
                return true;

            lock (_synchRoot)
            {
                info = GetInitializedServiceInfo(service);
                return info.TryGetRegistration(out registration);
            }
        }

        /// <inheritdoc />
        public bool IsRegistered(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var info = GetInitializedServiceInfoOrDefault(service);
            if (info != null && info.IsRegistered)
                return true;

            lock (_synchRoot)
            {
                return GetInitializedServiceInfo(service).IsRegistered;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var info = GetInitializedServiceInfoOrDefault(service);
            if (info != null)
                return info.Implementations.ToArray();

            lock (_synchRoot)
            {
                info = GetInitializedServiceInfo(service);
                return info.Implementations.ToArray();
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IComponentRegistration> DecoratorsFor(IServiceWithType service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            return _decorators.GetOrAdd(service, s =>
                RegistrationsFor(new DecoratorService(s.ServiceType))
                    .Where(r => !r.IsAdapterForIndividualComponent)
                    .OrderBy(r => r.GetRegistrationOrder())
                    .ToArray());
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            foreach (var registration in _registrations)
                registration.Dispose();

            base.Dispose(disposing);
        }

        private void UpdateInitializedAdapters(IComponentRegistration registration)
        {
            var adapterServices = new List<Service>();
            foreach (var serviceInfo in _serviceInfo)
            {
                if (serviceInfo.Value.ShouldRecalculateAdaptersOn(registration))
                {
                    adapterServices.Add(serviceInfo.Key);
                }
            }

            if (adapterServices.Count == 0)
                return;

            Debug.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "[Autofac] Component '{0}' provides services that have already been adapted. Consider refactoring to ContainerBuilder.Build() rather than Update().",
                    registration));

            var adaptationSandbox = new AdaptationSandbox(
                _dynamicRegistrationSources.Where(rs => rs.IsAdapterForIndividualComponents),
                registration,
                adapterServices);

            // Adapter registrations come from sources, so they are added with originatedFromSource: true
            var adapters = adaptationSandbox.GetAdapters();
            foreach (var adapter in adapters)
                AddRegistration(adapter, true, true);
        }

        private ServiceRegistrationInfo GetInitializedServiceInfo(Service service)
        {
            var info = GetServiceInfo(service);
            if (info.IsInitialized)
                return info;

            if (!info.IsInitializing)
                info.BeginInitialization(_dynamicRegistrationSources);

            while (info.HasSourcesToQuery)
            {
                var next = info.DequeueNextSource();
                foreach (var provided in next.RegistrationsFor(service, s => RegistrationsFor(s)))
                {
                    // This ensures that multiple services provided by the same
                    // component share a single component (we don't re-query for them)
                    foreach (var additionalService in provided.Services)
                    {
                        var additionalInfo = GetServiceInfo(additionalService);
                        if (additionalInfo.IsInitialized || additionalInfo == info) continue;

                        if (!additionalInfo.IsInitializing)
                            additionalInfo.BeginInitialization(_dynamicRegistrationSources.Where(src => src != next));
                        else
                            additionalInfo.SkipSource(next);
                    }

                    AddRegistration(provided, true, true);
                }
            }

            info.CompleteInitialization();
            return info;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ServiceRegistrationInfo GetServiceInfo(Service service)
        {
            if (_serviceInfo.TryGetValue(service, out var existing))
                return existing;

            var info = new ServiceRegistrationInfo(service);
            _serviceInfo.TryAdd(service, info);
            return info;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ServiceRegistrationInfo GetInitializedServiceInfoOrDefault(Service service)
        {
            if (_serviceInfo.TryGetValue(service, out var existing) && existing.IsInitialized)
                return existing;

            return null;
        }
    }
}