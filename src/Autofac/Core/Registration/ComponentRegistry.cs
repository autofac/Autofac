// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
    /// Maps services onto the components that provide them.
    /// </summary>
    /// <remarks>
    /// The component registry provides services directly from components,
    /// and also uses <see cref="IRegistrationSource"/> to generate components
    /// on-the-fly or as adapters for other components. A component registry
    /// is normally used through a <see cref="ContainerBuilder"/>, and not
    /// directly by application code.
    /// </remarks>
    public class ComponentRegistry : Disposable, IComponentRegistry
    {
        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        private readonly object _synchRoot = new object();

        /// <summary>
        /// External registration sources.
        /// </summary>
        private readonly List<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// All registrations.
        /// </summary>
        private readonly List<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        /// <summary>
        /// Keeps track of the status of registered services.
        /// </summary>
        private readonly ConcurrentDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new ConcurrentDictionary<Service, ServiceRegistrationInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistry"/> class.
        /// </summary>
        public ComponentRegistry()
            : this(new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistry"/> class.
        /// </summary>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistry(IDictionary<string, object> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        public IDictionary<string, object> Properties { get; }

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

        /// <summary>
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
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

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
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

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        public void Register(IComponentRegistration registration)
        {
            Register(registration, false);
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        /// <param name="preserveDefaults">If true, existing defaults for the services provided by the
        /// component will not be changed.</param>
        public void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            lock (_synchRoot)
            {
                AddRegistration(registration, preserveDefaults);
                UpdateInitialisedAdapters(registration);
            }
        }

        private void UpdateInitialisedAdapters(IComponentRegistration registration)
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

        protected virtual void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
        {
            foreach (var service in registration.Services)
            {
                var info = GetServiceInfo(service);
                info.AddImplementation(registration, preserveDefaults, originatedFromSource);
            }

            _registrations.Add(registration);

            GetRegistered()?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
        }

        /// <summary>
        /// Gets the registered components.
        /// </summary>
        public IEnumerable<IComponentRegistration> Registrations
        {
            get
            {
                lock (_synchRoot)
                    return _registrations.ToArray();
            }
        }

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
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

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        public event EventHandler<ComponentRegisteredEventArgs> Registered
        {
            add
            {
                lock (_synchRoot)
                {
                    Properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() + value;
                }
            }

            remove
            {
                lock (_synchRoot)
                {
                    Properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() - value;
                }
            }
        }

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        public void AddRegistrationSource(IRegistrationSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            lock (_synchRoot)
            {
                _dynamicRegistrationSources.Insert(0, source);
                foreach (var serviceRegistrationInfo in _serviceInfo)
                    serviceRegistrationInfo.Value.Include(source);

                var handler = RegistrationSourceAdded;
                handler?.Invoke(this, new RegistrationSourceAddedEventArgs(this, source));
            }
        }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the registry contains its own components.
        /// True if the registry contains its own components; false if it is forwarding
        /// registrations from another external registry.
        /// </summary>
        /// <remarks>This property is used when walking up the scope tree looking for
        /// registrations for a new customised scope.</remarks>
        public bool HasLocalComponents => true;

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;

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
        private ServiceRegistrationInfo GetInitializedServiceInfoOrDefault(Service service)
        {
            if (_serviceInfo.TryGetValue(service, out var existing) && existing.IsInitialized)
                return existing;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<ComponentRegisteredEventArgs> GetRegistered()
        {
            if (Properties.TryGetValue(MetadataKeys.RegisteredPropertyKey, out var registered))
                return (EventHandler<ComponentRegisteredEventArgs>)registered;

            return null;
        }
    }
}