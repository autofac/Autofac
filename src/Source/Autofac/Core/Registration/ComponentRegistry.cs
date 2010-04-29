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
        readonly IList<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// All registrations.
        /// </summary>
        readonly ICollection<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        /// <summary>
        /// Tracks the services known to the registry.
        /// </summary>
        class ServiceInfo
        {
            readonly Service _service;

            readonly LinkedList<IComponentRegistration> _implementations = new LinkedList<IComponentRegistration>();

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
            /// </summary>
            /// <param name="service">The tracked service.</param>
            public ServiceInfo(Service service)
            {
                _service = service;
            }

            /// <summary>
            /// The first time a service is requested, initialization (e.g. reading from sources)
            /// happens. This value will then be set to true. Calling many methods on this type before
            /// initialisation is an error.
            /// </summary>
            public bool IsInitialized { get; set; }

            /// <summary>
            /// The known implementations.
            /// </summary>
            public IEnumerable<IComponentRegistration> Implementations
            { 
                get
                {
                    RequiresInitialization();
                    return _implementations; 
                }
            }

            void RequiresInitialization()
            {
                if (!IsInitialized)
                    throw new InvalidOperationException();
            }

            /// <summary>
            /// True if any implementations are known.
            /// </summary>
            public bool IsRegistered
            { 
                get 
                {
                    RequiresInitialization();
                    return Any; 
                } 
            }

            bool Any { get { return _implementations.First != null; } }

            /// <summary>
            /// Used for bookkeeping so that the same source is not queried twice (may be null.)
            /// </summary>
            public Queue<IRegistrationSource> SourcesToQuery { get; set; }

            public void AddImplementation(IComponentRegistration registration, bool preserveDefaults)
            {
                if (preserveDefaults)
                {
                    _implementations.AddLast(registration);
                }
                else
                {
                    if (Any)
                        Debug.WriteLine(string.Format(
                                "[Autofac] Overriding default for: '{0}' with: '{1}' (was '{2}')",
                                _service, registration, _implementations.First));

                    _implementations.AddFirst(registration);
                }
            }

            public bool TryGetRegistration(out IComponentRegistration registration)
            {
                RequiresInitialization();

                if (Any)
                {
                    registration = _implementations.First.Value;
                    return true;
                }

                registration = null;
                return false;
            }
        }

        /// <summary>
        /// Keeps track of the status of registered services.
        /// </summary>
        readonly IDictionary<Service, ServiceInfo> _serviceInfo = new Dictionary<Service, ServiceInfo>();

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
        /// <param name="source"></param>
        public void AddRegistrationSource(IRegistrationSource source)
        {
            Enforce.ArgumentNotNull(source, "source");

            lock (_synchRoot)
            {
                _dynamicRegistrationSources.Insert(0, source);
            }
        }

        ServiceInfo GetInitializedServiceInfo(Service service)
        {
            var info = GetServiceInfo(service);
            if (info.IsInitialized)
                return info;

            if (info.SourcesToQuery == null)
                info.SourcesToQuery = new Queue<IRegistrationSource>(_dynamicRegistrationSources);

            while (info.SourcesToQuery != null && info.SourcesToQuery.Count != 0)
            {
                var next = info.SourcesToQuery.Dequeue();
                foreach (var provided in next.RegistrationsFor(service, RegistrationsFor))
                {
                    // This ensures that multiple services provided by the same
                    // component share a single component (we don't re-query for them)
                    foreach (var additionalService in provided.Services)
                    {
                        var additionalInfo = GetServiceInfo(additionalService);
                        if (additionalInfo.IsInitialized) continue;

                        if (additionalInfo.SourcesToQuery == null)
                            additionalInfo.SourcesToQuery = new Queue<IRegistrationSource>(
                                _dynamicRegistrationSources.Where(src => src != next));
                        else
                            additionalInfo.SourcesToQuery = new Queue<IRegistrationSource>(
                                additionalInfo.SourcesToQuery.Where(src => src != next));
                    }

                    Register(provided, true);
                }
            }

            info.IsInitialized = true;
            info.SourcesToQuery = null;
            return info;
        }

        ServiceInfo GetServiceInfo(Service service)
        {
            ServiceInfo existing;
            if (_serviceInfo.TryGetValue(service, out existing))
                return existing;

            var info = new ServiceInfo(service);
            _serviceInfo.Add(service, info);
            return info;
        }
    }
}
