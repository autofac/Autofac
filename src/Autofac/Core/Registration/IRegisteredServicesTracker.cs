// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core.Resolving.Pipeline;

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
        /// <param name="originatedFromDynamicSource">Indicates whether this is an explicitly added registration or that it has been added by a dynamic registration source.</param>
        void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromDynamicSource = false);

        /// <summary>
        /// Adds a piece of middleware to a service.
        /// </summary>
        /// <param name="service">The service to add to.</param>
        /// <param name="middleware">The middleware.</param>
        /// <param name="insertionMode">The insertion mode.</param>
        void AddServiceMiddleware(Service service, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase);

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void AddRegistrationSource(IRegistrationSource source);

        /// <summary>
        /// Adds a service middleware source that will provide service middleware on-the-fly.
        /// </summary>
        /// <param name="serviceMiddlewareSource">The source to register.</param>
        void AddServiceMiddlewareSource(IServiceMiddlewareSource serviceMiddlewareSource);

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via an <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<IComponentRegistration> Registered;

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<IRegistrationSource> RegistrationSourceAdded;

        /// <summary>
        /// Should be called prior to the construction of a <see cref="ComponentRegistry" /> to
        /// indicate that the tracker is complete, and requested service information should no longer be ephemeral.
        /// </summary>
        void Complete();

        /// <summary>
        /// Gets the registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> Registrations { get; }

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        IEnumerable<IRegistrationSource> Sources { get; }

        /// <summary>
        /// Gets the set of registered service middleware sources.
        /// </summary>
        IEnumerable<IServiceMiddlewareSource> ServiceMiddlewareSources { get; }

        /// <summary>
        /// Gets the set of configured service middleware for a service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <returns>The set of middleware.</returns>
        IEnumerable<IResolveMiddleware> ServiceMiddlewareFor(Service service);

        /// <summary>
        /// Attempts to find a default service registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="serviceRegistration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        bool TryGetServiceRegistration(Service service, out ServiceRegistration serviceRegistration);

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
