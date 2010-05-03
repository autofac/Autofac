// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Provides component registrations according to the services they provide.
    /// </summary>
    public class ComponentRegistry : Disposable, IComponentRegistry
    {
        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        readonly object _synchRoot = new object();

        /// <summary>
        /// External registration sources.
        /// </summary>
        readonly IList<KeyValuePair<IRegistrationSource, bool>> _dynamicRegistrationSources = new List<KeyValuePair<IRegistrationSource, bool>>();

        /// <summary>
        /// All registrations.
        /// </summary>
        readonly ICollection<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        /// <summary>
        /// Keeps track of the status of registered services.
        /// </summary>
        readonly IDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new Dictionary<Service, ServiceRegistrationInfo>();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
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
            Enforce.ArgumentNotNull(service, "service");
            lock (_synchRoot)
            {
                var info = GetInitializedServiceInfo(service);
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
            Enforce.ArgumentNotNull(service, "service");
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
        public virtual void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            Enforce.ArgumentNotNull(registration, "registration");

            AddRegistration(registration, preserveDefaults);

            UpdateInitialisedAdapters(registration);
        }

        void UpdateInitialisedAdapters(IComponentRegistration registration)
        {
            var adapterServices = _serviceInfo
                .Where(si => si.Value.AdaptsAnyServicesOf(registration))
                .Select(si => si.Key)
                .ToArray();

            if (adapterServices.Length != 0)
            {
                Debug.WriteLine(String.Format(
                    "[Autofac] Component '{0}' provides services that have already been adapted. Consider refactoring to ContainerBuilder.Build() rather than Update().",
                    registration));

                var adaptationSandbox = new AdaptationSandbox(
                    _dynamicRegistrationSources.Where(rs => rs.Value).Select(rs => rs.Key),
                    registration,
                    adapterServices);

                var adapters = adaptationSandbox.GetAdapters();
                foreach (var adapter in adapters)
                    AddRegistration(adapter, true);
            }
        }

        void AddRegistration(IComponentRegistration registration, bool preserveDefaults)
        {
            foreach (var service in registration.Services)
            {
                var info = GetServiceInfo(service);
                info.AddImplementation(registration, preserveDefaults);
            }

            _registrations.Add(registration);

            Registered(this, new ComponentRegisteredEventArgs(this, registration));
        }

        /// <summary>
        /// Enumerate the registered components.
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
            Enforce.ArgumentNotNull(service, "service");
            lock (_synchRoot)
            {
                var info = GetInitializedServiceInfo(service);
                return info.Implementations.ToArray();
            }
        }

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        public event EventHandler<ComponentRegisteredEventArgs> Registered = (s, e) => { };

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        /// <param name="isAdapter">Whether the registration source creates new
        /// components with a 1:1 relationship to other components.</param>
        public void AddRegistrationSource(IRegistrationSource source, bool isAdapter)
        {
            Enforce.ArgumentNotNull(source, "source");

            lock (_synchRoot)
            {
                _dynamicRegistrationSources.Insert(0, new KeyValuePair<IRegistrationSource, bool>(source, isAdapter));
                foreach (var serviceRegistrationInfo in _serviceInfo)
                    serviceRegistrationInfo.Value.Include(source);
            }
        }

        ServiceRegistrationInfo GetInitializedServiceInfo(Service service)
        {
            var info = GetServiceInfo(service);
            if (info.IsInitialized)
                return info;

            if (!info.IsInitializing)
                info.BeginInitialization(_dynamicRegistrationSources.Select(rs => rs.Key));

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
                        if (additionalInfo.IsInitialized) continue;

                        if (!additionalInfo.IsInitializing)
                            additionalInfo.BeginInitialization(_dynamicRegistrationSources
                                .Select(rs => rs.Key).Where(src => src != next));
                        else
                            additionalInfo.SkipSource(next);
                    }

                    AddRegistration(provided, true);
                }
            }

            info.CompleteInitialization();
            return info;
        }

        ServiceRegistrationInfo GetServiceInfo(Service service)
        {
            ServiceRegistrationInfo existing;
            if (_serviceInfo.TryGetValue(service, out existing))
                return existing;

            var info = new ServiceRegistrationInfo(service);
            _serviceInfo.Add(service, info);
            return info;
        }
    }
}
