using System;
using System.Collections.Generic;

namespace Autofac.Core
{
    public interface IImmutableComponentRegistry : IDisposable
    {
        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IReadOnlyDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the set of registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> Registrations { get; }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        IEnumerable<IRegistrationSource> Sources { get; }

        /// <summary>
        /// Gets a value indicating whether the registry contains its own components.
        /// True if the registry contains its own components; false if it is forwarding
        /// registrations from another external registry.
        /// </summary>
        /// <remarks>This property is used when walking up the scope tree looking for
        /// registrations for a new customised scope.</remarks>
        bool HasLocalComponents { get; }

        /// <summary>
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool TryGetRegistration(Service service, out IComponentRegistration registration);

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
        /// Selects all available decorator registrations that can be applied to the specified registration.
        /// </summary>
        /// <param name="registration">The registration for which decorator registrations are sought.</param>
        /// <returns>Decorator registrations applicable to <paramref name="registration"/>.</returns>
        IEnumerable<IComponentRegistration> DecoratorsFor(IComponentRegistration registration);
    }
}