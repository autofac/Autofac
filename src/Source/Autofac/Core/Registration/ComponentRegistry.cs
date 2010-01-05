// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
        /// Associates each service with the registrations that
        /// can provide that service. The first item in the list is considered to be the default.
        /// </summary>
        readonly IDictionary<Service, LinkedList<IComponentRegistration>> _registrationsByService = new Dictionary<Service, LinkedList<IComponentRegistration>>();

        /// <summary>
        /// All registrations.
        /// </summary>
        readonly ICollection<IComponentRegistration> _registrations = new List<IComponentRegistration>();

        /// <summary>
        /// Filtering prevents stack overflow when sources filter for the same service that they were
        /// queried for.
        /// </summary>
        static readonly Func<IRegistrationSource, Service, bool> EmptyFilter = (r, s) => true;

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
                return IsRegistered(service, EmptyFilter);
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
                return TryGetRegistration(service, EmptyFilter, out registration);
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
        /// <param name="preserveDefaults">If true, existing defaults for any services provided
        /// by the component will not be changed.</param>
        public void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            Enforce.ArgumentNotNull(registration, "registration");

            lock (_synchRoot)
            {
                RegisterWithoutReTestingSources(registration, preserveDefaults);
                foreach (var service in registration.Services)
                    RegisterFromSources(service, EmptyFilter);
            }
        }

        /// <summary>
        /// Enumerate the registered components.
        /// </summary>
        /// <remarks>
        /// Component registrations are generated on-the-fly for some service types
        /// (e.g. open generics.) This means that the
        /// available registrations will grow over time as instances of these service
        /// types are resolved. To get all registrations for a service, use
        /// <see cref="RegistrationsFor(Service)"/>. To handle all registered components,
        /// override <see cref="Module.AttachToComponentRegistration"/>.
        /// </remarks>
        public IEnumerable<IComponentRegistration> Registrations
        {
            get
            {
                lock(_synchRoot)
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
            return RegistrationsFor(service, EmptyFilter);
        }

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source"></param>
        public void AddRegistrationSource(IRegistrationSource source)
        {
            Enforce.ArgumentNotNull(source, "source");

            lock (_synchRoot)
            {
                var newRegistrations = _registrationsByService.Keys.Concat(_unregisteredServices)
                    .SelectMany(existing => InvokeSource(source, existing, EmptyFilter));

                foreach (var r in newRegistrations)
                    RegisterWithoutReTestingSources(r, true);

                _dynamicRegistrationSources.Add(source);
            }
        }

        IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<IRegistrationSource, Service, bool> filter)
        {
            lock (_synchRoot)
            {
                if (IsRegistered(service, filter))
                    return _registrationsByService[service].ToArray();

                return Enumerable.Empty<IComponentRegistration>();
            }
        }

        IEnumerable<IComponentRegistration> InvokeSource(IRegistrationSource source, Service service, Func<IRegistrationSource, Service, bool> filter)
        {
            Func<IRegistrationSource, Service, bool> filterIncludingCurrent =
                (rs, s) => !(rs == source && s == service) && filter(rs, s);

            return source.RegistrationsFor(service, s =>
            {
                if (filterIncludingCurrent(source, s))
                    return RegistrationsFor(s, filterIncludingCurrent);
                
                return Enumerable.Empty<IComponentRegistration>();
            });
        }

        bool IsRegistered(Service service, Func<IRegistrationSource, Service, bool> filter)
        {
            IComponentRegistration unused;
            return TryGetRegistration(service, filter, out unused);
        }

        bool TryGetRegistration(Service service, Func<IRegistrationSource, Service, bool> filter, out IComponentRegistration registration)
        {
            if (TryGetExistingDefault(service, out registration))
                return true;

            if (_unregisteredServices.Contains(service))
                return false;

            var results = RegisterFromSources(service, filter);

            if (!results.Any())
            {
                _unregisteredServices.Add(service);
                return false;
            }

            return TryGetExistingDefault(service, out registration);
        }

        bool TryGetExistingDefault(Service service, out IComponentRegistration registration)
        {
            LinkedList<IComponentRegistration> registrations;
            if (_registrationsByService.TryGetValue(service, out registrations))
            {
                registration = registrations.First.Value;
                return true;
            }

            registration = null;
            return false;
        }

        IEnumerable<IComponentRegistration> RegisterFromSources(Service service, Func<IRegistrationSource, Service, bool> filter)
        {
            var results = _dynamicRegistrationSources
                .SelectMany(rs => InvokeSource(rs, service, filter))
                .ToArray();

            foreach (var r in results)
                RegisterWithoutReTestingSources(r, true);

            return results;
        }

        void RegisterWithoutReTestingSources(IComponentRegistration registration, bool preserveDefaults)
        {
            foreach (var service in registration.Services)
            {
                LinkedList<IComponentRegistration> existing;
                if (_registrationsByService.TryGetValue(service, out existing))
                {
                    if (!preserveDefaults)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format(
                            "[Autofac] Overriding existing registration for: '{0}' with: '{1}' (was: '{2}')",
                            service, registration, existing));
                    }
                }
                else
                {
                    existing = new LinkedList<IComponentRegistration>();
                    _registrationsByService.Add(service, existing);
                }

                if (preserveDefaults)
                    existing.AddLast(registration);
                else
                    existing.AddFirst(registration);

                _unregisteredServices.Remove(service);
            }

            _registrations.Add(registration);

            Registered(this, new ComponentRegisteredEventArgs(this, registration));
        }
    }
}
