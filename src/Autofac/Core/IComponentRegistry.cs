// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// Provides component registrations according to the services they provide.
    /// </summary>
    public interface IComponentRegistry : IDisposable
    {
        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        IDictionary<string, object?> Properties { get; }

        /// <summary>
        /// Gets the set of registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> Registrations { get; }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        IEnumerable<IRegistrationSource> Sources { get; }

        /// <summary>
        /// Gets the set of service middleware sources that are used by the registry.
        /// </summary>
        IEnumerable<IServiceMiddlewareSource> ServiceMiddlewareSources { get; }

        /// <summary>
        /// Gets a value indicating whether the registry contains its own components.
        /// True if the registry contains its own components; false if it is forwarding
        /// registrations from another external registry.
        /// </summary>
        /// <remarks>This property is used when walking up the scope tree looking for
        /// registrations for a new customized scope.</remarks>
        bool HasLocalComponents { get; }

        /// <summary>
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool TryGetRegistration(Service service, [NotNullWhen(returnValue: true)] out IComponentRegistration? registration);

        /// <summary>
        /// Attempts to find a default service registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="serviceRegistration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool TryGetServiceRegistration(Service service, out ServiceRegistration serviceRegistration);

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(Service service);

        /// <summary>
        /// Gets the set of custom service middleware for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <returns>The set of custom service middleware.</returns>
        IEnumerable<IResolveMiddleware> ServiceMiddlewareFor(Service service);

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
        IEnumerable<IComponentRegistration> RegistrationsFor(Service service);

        /// <summary>
        /// Selects from the available service registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Service registrations supporting <paramref name="service"/>.</returns>
        IEnumerable<ServiceRegistration> ServiceRegistrationsFor(Service service);
    }
}
