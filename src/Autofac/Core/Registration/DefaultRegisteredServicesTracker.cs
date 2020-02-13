using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share context across registrations.
        /// </value>
        private readonly IDictionary<string, object?> _properties = new Dictionary<string, object?>();

        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        private readonly object _synchRoot = new object();

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        public event EventHandler<IComponentRegistration> Registered
        {
            add => _properties[MetadataKeys.InternalRegisteredPropertyKey] = GetRegistered() + value;

            remove => _properties[MetadataKeys.InternalRegisteredPropertyKey] = GetRegistered() - value;
        }

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        public event EventHandler<IRegistrationSource> RegistrationSourceAdded
        {
            add => _properties[MetadataKeys.InternalRegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() + value;

            remove => _properties[MetadataKeys.InternalRegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() - value;
        }

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
            GetRegistered()?.Invoke(this, registration);
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

                var handler = GetRegistrationSourceAdded();
                handler?.Invoke(this, source);
            }
        }

        /// <inheritdoc />
        public bool TryGetRegistration(Service service, [NotNullWhen(returnValue: true)] out IComponentRegistration? registration)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            lock (_synchRoot)
            {
                var info = GetInitializedServiceInfo(service);
                return info.TryGetRegistration(out registration);
            }
        }

        /// <inheritdoc />
        public bool IsRegistered(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            lock (_synchRoot)
            {
                return GetInitializedServiceInfo(service).IsRegistered;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            lock (_synchRoot)
            {
                var info = GetInitializedServiceInfo(service);
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
                foreach (var provided in next.RegistrationsFor(service, RegistrationsFor))
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
        private EventHandler<IComponentRegistration>? GetRegistered() =>
            _properties.TryGetValue(MetadataKeys.InternalRegisteredPropertyKey, out var registered)
                ? (EventHandler<IComponentRegistration>?)registered
                : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<IRegistrationSource>? GetRegistrationSourceAdded() =>
            _properties.TryGetValue(MetadataKeys.InternalRegistrationSourceAddedPropertyKey, out var registrationSourceAdded)
                ? (EventHandler<IRegistrationSource>?)registrationSourceAdded
                : null;
    }
}