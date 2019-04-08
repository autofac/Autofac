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
using System.Collections.ObjectModel;
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
    internal class ComponentRegistry : Disposable, IComponentRegistry, IComponentRegistryBuilder
    {
        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        private readonly object _synchRoot = new object();

        private readonly ConcurrentDictionary<IComponentRegistration, IEnumerable<IComponentRegistration>> _decorators
            = new ConcurrentDictionary<IComponentRegistration, IEnumerable<IComponentRegistration>>();

        private readonly IDictionary<string, object> _properties;
        private readonly IRegisteredServicesTracker _registeredServicesTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistry"/> class.
        /// </summary>
        /// <param name="registeredServicesTracker">The tracker for the registered services.</param>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistry(IRegisteredServicesTracker registeredServicesTracker, IDictionary<string, object> properties)
        {
            _properties = properties;
            _registeredServicesTracker = registeredServicesTracker;
            Register(new SelfComponentRegistration());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistry"/> class.
        /// </summary>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistry(IDictionary<string, object> properties)
            : this(new DefaultRegisteredServicesTracker(), properties)
        {
        }

        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        IReadOnlyDictionary<string, object> IComponentRegistry.Properties
        {
            get { return new ReadOnlyDictionary<string, object>(_properties); }
        }

        IDictionary<string, object> IComponentRegistryBuilder.Properties
        {
            get { return _properties; }
        }

        IComponentRegistry IComponentRegistryBuilder.Build()
        {
            return this;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _registeredServicesTracker.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool IComponentRegistry.TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            return _registeredServicesTracker.TryGetRegistration(service, out registration);
        }

        bool IComponentRegistryBuilder.IsRegistered(Service service)
        {
            return _registeredServicesTracker.IsRegistered(service);
        }

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        bool IComponentRegistry.IsRegistered(Service service)
        {
            return _registeredServicesTracker.IsRegistered(service);
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        void IComponentRegistryBuilder.Register(IComponentRegistration registration)
        {
            Register(registration);
        }

        private void Register(IComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            _registeredServicesTracker.AddRegistration(registration, false);
            GetRegistered()?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        /// <param name="preserveDefaults">If true, existing defaults for the services provided by the
        /// component will not be changed.</param>
        void IComponentRegistryBuilder.Register(IComponentRegistration registration, bool preserveDefaults)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            _registeredServicesTracker.AddRegistration(registration, preserveDefaults);
            GetRegistered()?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
        }

        /// <summary>
        /// Gets the registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> IComponentRegistry.Registrations
        {
            get { return _registeredServicesTracker.Registrations; }
        }

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
        IEnumerable<IComponentRegistration> IComponentRegistry.RegistrationsFor(Service service)
        {
            return _registeredServicesTracker.RegistrationsFor(service);
        }

        /// <inheritdoc />
        IEnumerable<IComponentRegistration> IComponentRegistry.DecoratorsFor(IComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return _decorators.GetOrAdd(registration, r =>
            {
                var result = new List<IComponentRegistration>();

                foreach (var service in r.Services)
                {
                    if (service is DecoratorService || !(service is IServiceWithType swt)) continue;

                    var decoratorService = new DecoratorService(swt.ServiceType);
                    var decoratorRegistrations = _registeredServicesTracker.RegistrationsFor(decoratorService);
                    result.AddRange(decoratorRegistrations);
                }

                return result.OrderBy(d => d.GetRegistrationOrder()).ToArray();
            });
        }

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<ComponentRegisteredEventArgs> IComponentRegistryBuilder.Registered
        {
            add
            {
                lock (_synchRoot)
                {
                    foreach (IComponentRegistration registration in _registeredServicesTracker.Registrations)
                    {
                        value(this, new ComponentRegisteredEventArgs(this, registration));
                    }

                    _properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() + value;
                }
            }

            remove
            {
                lock (_synchRoot)
                {
                    _properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() - value;
                }
            }
        }

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void IComponentRegistryBuilder.AddRegistrationSource(IRegistrationSource source)
        {
            _registeredServicesTracker.AddRegistrationSource(source);
            GetRegistrationSourceAdded()?.Invoke(this, new RegistrationSourceAddedEventArgs(this, source));
        }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        IEnumerable<IRegistrationSource> IComponentRegistry.Sources
        {
            get { return _registeredServicesTracker.Sources; }
        }

        /// <summary>
        /// Gets a value indicating whether the registry contains its own components.
        /// True if the registry contains its own components; false if it is forwarding
        /// registrations from another external registry.
        /// </summary>
        /// <remarks>This property is used when walking up the scope tree looking for
        /// registrations for a new customised scope.</remarks>
        bool IComponentRegistry.HasLocalComponents => true;

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<RegistrationSourceAddedEventArgs> IComponentRegistryBuilder.RegistrationSourceAdded
        {
            add
            {
                lock (_synchRoot)
                {
                    foreach (IRegistrationSource source in _registeredServicesTracker.Sources)
                    {
                        value(this, new RegistrationSourceAddedEventArgs(this, source));
                    }

                    _properties[MetadataKeys.RegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() + value;
                }
            }

            remove
            {
                lock (_synchRoot)
                {
                    _properties[MetadataKeys.RegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() - value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<ComponentRegisteredEventArgs> GetRegistered()
        {
            if (_properties.TryGetValue(MetadataKeys.RegisteredPropertyKey, out var registered))
                return (EventHandler<ComponentRegisteredEventArgs>)registered;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<RegistrationSourceAddedEventArgs> GetRegistrationSourceAdded()
        {
            if (_properties.TryGetValue(MetadataKeys.RegistrationSourceAddedPropertyKey, out var registrationSourceAdded))
                return (EventHandler<RegistrationSourceAddedEventArgs>)registrationSourceAdded;

            return null;
        }
    }
}