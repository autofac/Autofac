using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Keeps track of the status of registered services.
    /// </summary>
    internal interface IRegisteredServicesTracker : IDisposable
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
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool TryGetRegistration(Service service, [NotNullWhen(returnValue: true)] out IComponentRegistration? registration);

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
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(Service service);

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
        IEnumerable<IComponentRegistration> RegistrationsFor(Service service);

        /// <summary>
        /// Selects all available decorator registrations that can be applied to the specified service.
        /// </summary>
        /// <param name="service">The service for which decorator registrations are sought.</param>
        /// <returns>Decorator registrations applicable to <paramref name="service"/>.</returns>
        IReadOnlyList<IComponentRegistration> DecoratorsFor(IServiceWithType service);
    }
}