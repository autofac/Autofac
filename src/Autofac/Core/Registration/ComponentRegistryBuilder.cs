using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using Autofac.Builder;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    internal class ComponentRegistryBuilder : Disposable, IComponentRegistryBuilder
    {
        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        private readonly object _synchRoot = new object();

        private readonly IDictionary<string, object> _properties;
        private readonly IRegisteredServicesTracker _registeredServicesTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistryBuilder"/> class.
        /// </summary>
        /// <param name="registeredServicesTracker">The tracker for the registered services.</param>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistryBuilder(IRegisteredServicesTracker registeredServicesTracker, IDictionary<string, object> properties)
        {
            _properties = properties;
            _registeredServicesTracker = registeredServicesTracker;
            Register(new SelfComponentRegistration());
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

        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        public IComponentRegistry Build()
        {
            return new ComponentRegistry(_registeredServicesTracker, Properties);
        }

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
            GetRegistered()?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
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
            GetRegistered()?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
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
        public void AddRegistrationSource(IRegistrationSource source)
        {
            _registeredServicesTracker.AddRegistrationSource(source);
            GetRegistrationSourceAdded()?.Invoke(this, new RegistrationSourceAddedEventArgs(this, source));
        }

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
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