using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Autofac.Builder;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Used to build a <see cref="IComponentRegistry" />.
    /// </summary>
    internal class ComponentRegistryBuilder : Disposable, IComponentRegistryBuilder
    {
        /// <summary>
        /// The tracker for the registered services.
        /// </summary>
        private readonly IRegisteredServicesTracker _registeredServicesTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistryBuilder"/> class.
        /// </summary>
        /// <param name="registeredServicesTracker">The tracker for the registered services.</param>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistryBuilder(IRegisteredServicesTracker registeredServicesTracker, IDictionary<string, object?> properties)
        {
            Properties = properties;
            _registeredServicesTracker = registeredServicesTracker;
            _registeredServicesTracker.Registered += OnRegistered;
            _registeredServicesTracker.RegistrationSourceAdded += OnRegistrationSourceAdded;
        }

        private void OnRegistered(object sender, IComponentRegistration e)
        {
            var handler = GetRegistered();

            handler?.Invoke(this, new ComponentRegisteredEventArgs(this, e));
        }

        private void OnRegistrationSourceAdded(object sender, IRegistrationSource e)
        {
            var handler = GetRegistrationSourceAdded();

            handler?.Invoke(this, new RegistrationSourceAddedEventArgs(this, e));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _registeredServicesTracker.Registered -= OnRegistered;
            _registeredServicesTracker.RegistrationSourceAdded -= OnRegistrationSourceAdded;
            _registeredServicesTracker.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        public IDictionary<string, object?> Properties { get; }

        /// <summary>
        /// Create a new <see cref="IComponentRegistry" /> with all the component registrations that have been made.
        /// </summary>
        /// <returns>A new component registry with the configured component registrations.</returns>
        public IComponentRegistry Build()
        {
            return new ComponentRegistry(_registeredServicesTracker, Properties);
        }

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(Service service)
        {
            return _registeredServicesTracker.IsRegistered(service);
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        public void Register(IComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            _registeredServicesTracker.AddRegistration(registration, false);
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

            _registeredServicesTracker.AddRegistration(registration, preserveDefaults);
        }

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        public event EventHandler<ComponentRegisteredEventArgs> Registered
        {
            add
            {
                foreach (IComponentRegistration registration in _registeredServicesTracker.Registrations)
                {
                    value(this, new ComponentRegisteredEventArgs(this, registration));
                }

                Properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() + value;
            }

            remove
            {
                Properties[MetadataKeys.RegisteredPropertyKey] = GetRegistered() - value;
            }
        }

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        public void AddRegistrationSource(IRegistrationSource source)
        {
            _registeredServicesTracker.AddRegistrationSource(source);
        }

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
        {
            add
            {
                foreach (IRegistrationSource source in _registeredServicesTracker.Sources)
                {
                    value(this, new RegistrationSourceAddedEventArgs(this, source));
                }

                Properties[MetadataKeys.RegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() + value;
            }

            remove
            {
                Properties[MetadataKeys.RegistrationSourceAddedPropertyKey] = GetRegistrationSourceAdded() - value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<ComponentRegisteredEventArgs>? GetRegistered()
        {
            if (Properties.TryGetValue(MetadataKeys.RegisteredPropertyKey, out var registered))
                return (EventHandler<ComponentRegisteredEventArgs>?)registered;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventHandler<RegistrationSourceAddedEventArgs>? GetRegistrationSourceAdded()
        {
            if (Properties.TryGetValue(MetadataKeys.RegistrationSourceAddedPropertyKey, out var registrationSourceAdded))
                return (EventHandler<RegistrationSourceAddedEventArgs>?)registrationSourceAdded;

            return null;
        }
    }
}