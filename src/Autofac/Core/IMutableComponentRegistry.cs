using System;
using System.Collections.Generic;

namespace Autofac.Core
{
    public interface IMutableComponentRegistry
    {
        IDictionary<string, object> MutableProperties { get; }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        void Register(IComponentRegistration registration);

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        /// <param name="preserveDefaults">If true, existing defaults for the services provided by the
        /// component will not be changed.</param>
        void Register(IComponentRegistration registration, bool preserveDefaults);

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<ComponentRegisteredEventArgs> Registered;

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void AddRegistrationSource(IRegistrationSource source);

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;
    }
}