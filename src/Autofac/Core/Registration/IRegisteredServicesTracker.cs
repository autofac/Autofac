using System;
using System.Collections.Generic;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Keeps track of the status of registered services.
    /// </summary>
    internal interface IRegisteredServicesTracker : IDisposable, IComponentRegistryServices
    {
        /// <summary>
        /// Adds a registration to the list of registered services.
        /// </summary>
        /// <param name="registration">The registration to add.</param>
        /// <param name="preserveDefaults">Indicates whether the defaults should be preserved.</param>
        /// <param name="originatedFromSource">Indicates whether this is an explicitly added registration or that it has been added by a different source.</param>
        void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false);

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void AddRegistrationSource(IRegistrationSource source);

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via an <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<IComponentRegistration> Registered;

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<IRegistrationSource> RegistrationSourceAdded;

        /// <summary>
        /// Gets the registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> Registrations { get; }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        IEnumerable<IRegistrationSource> Sources { get; }

        /// <summary>
        /// Selects all available decorator registrations that can be applied to the specified service.
        /// </summary>
        /// <param name="service">The service for which decorator registrations are sought.</param>
        /// <returns>Decorator registrations applicable to <paramref name="service"/>.</returns>
        IReadOnlyList<IComponentRegistration> DecoratorsFor(IServiceWithType service);
    }
}
