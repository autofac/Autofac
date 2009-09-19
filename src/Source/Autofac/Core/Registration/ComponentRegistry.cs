// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
        /// Services known not to be registered.
        /// </summary>
        readonly ICollection<Service> _unregisteredServices = new HashSet<Service>();
        
        /// <summary>
        /// External registration sources.
        /// </summary>
        readonly ICollection<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// Associates each service with the default registration that
        /// can provide that service.
        /// </summary>
        readonly IDictionary<Service, IComponentRegistration> _defaultRegistrations = new Dictionary<Service, IComponentRegistration>();

        /// <summary>
        /// All registrations.
        /// </summary>
        readonly ICollection<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        /// <summary>
        /// Constructs a new component registry.
        /// </summary>
        public ComponentRegistry()
        {
        }

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
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(Service service)
        {
            Enforce.ArgumentNotNull(service, "service");

            lock (_synchRoot)
            {
                IComponentRegistration unused;
                return TryGetRegistration(service, out unused);
            }
        }

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        public event EventHandler<ComponentRegisteredEventArgs> Registered = (s, e) => { };

        /// <summary>
        /// Gets the default component registration that will be used to satisfy
        /// requests for the provided service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="registration">The registration.</param>
        /// <returns>
        /// True if a default exists, false otherwise.
        /// </returns>
        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");

            lock (_synchRoot)
            {
                if (_defaultRegistrations.TryGetValue(service, out registration))
                    return true;

                if (_unregisteredServices.Contains(service))
                    return false;

                foreach (var rs in _dynamicRegistrationSources)
                {
                    if (rs.TryGetRegistration(service, s => IsRegistered(s), out registration)) 
                    {
                        Register(registration);
                        return true;
                    }
                }

                _unregisteredServices.Add(service);
                return false;
            }
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        public void Register(IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(registration, "registration");

            lock (_synchRoot)
            {
                foreach (var service in registration.Services)
                {
                    IComponentRegistration existing;
                    if (_defaultRegistrations.TryGetValue(service, out existing))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format(
                            "Autofac: Overriding existing registration for: '{0}' with: '{1}' (was: '{2}')",
                            service, registration, existing));
                    }

                    _defaultRegistrations[service] = registration;
                    _unregisteredServices.Remove(service);
                }

                _registrations.Add(registration);

                Registered(this, new ComponentRegisteredEventArgs(this, registration));
            }
        }

        /// <summary>
        /// Enumerate the registered components.
        /// </summary>
        public IEnumerable<IComponentRegistration> Registrations
        {
            get
            {
                lock(_synchRoot)
                    return _registrations.ToList();
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
                IsRegistered(service);
                return Registrations.Where(r => r.Services.Contains(service));
            }
        }

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source"></param>
        public void AddRegistrationSource(IRegistrationSource source)
        {
            lock (_synchRoot)
            {
                _dynamicRegistrationSources.Add(Enforce.ArgumentNotNull(source, "source"));
                _unregisteredServices.Clear();
            }
        }
    }
}
