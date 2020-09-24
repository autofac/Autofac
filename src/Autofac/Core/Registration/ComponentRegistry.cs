// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Resolving.Pipeline;
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
    internal class ComponentRegistry : Disposable, IComponentRegistry
    {
        private readonly IRegisteredServicesTracker _registeredServicesTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistry"/> class.
        /// </summary>
        /// <param name="registeredServicesTracker">The tracker for the registered services.</param>
        /// <param name="properties">The properties used during component registration.</param>
        internal ComponentRegistry(IRegisteredServicesTracker registeredServicesTracker, IDictionary<string, object?> properties)
        {
            Properties = properties;
            _registeredServicesTracker = registeredServicesTracker;
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
        /// Gets the registered components.
        /// </summary>
        public IEnumerable<IComponentRegistration> Registrations => _registeredServicesTracker.Registrations;

        /// <summary>
        /// Gets the registration sources that are used by the registry.
        /// </summary>
        public IEnumerable<IRegistrationSource> Sources => _registeredServicesTracker.Sources;

        /// <inheritdoc/>
        public IEnumerable<IServiceMiddlewareSource> ServiceMiddlewareSources => _registeredServicesTracker.ServiceMiddlewareSources;

        /// <summary>
        /// Gets a value indicating whether the registry contains its own components.
        /// True if the registry contains its own components; false if it is forwarding
        /// registrations from another external registry.
        /// </summary>
        /// <remarks>This property is used when walking up the scope tree looking for
        /// registrations for a new customized scope.</remarks>
        public bool HasLocalComponents => true;

        /// <summary>
        /// Attempts to find a default registration for the specified service.
        /// </summary>
        /// <param name="service">The service to look up.</param>
        /// <param name="registration">The default registration for the service.</param>
        /// <returns>True if a registration exists.</returns>
        public bool TryGetRegistration(Service service, [NotNullWhen(returnValue: true)] out IComponentRegistration? registration)
            => _registeredServicesTracker.TryGetRegistration(service, out registration);

        /// <inheritdoc/>
        public bool TryGetServiceRegistration(Service service, out ServiceRegistration serviceRegistration)
            => _registeredServicesTracker.TryGetServiceRegistration(service, out serviceRegistration);

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(Service service) => _registeredServicesTracker.IsRegistered(service);

        /// <inheritdoc/>
        public IEnumerable<IResolveMiddleware> ServiceMiddlewareFor(Service service) => _registeredServicesTracker.ServiceMiddlewareFor(service);

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
            => _registeredServicesTracker.RegistrationsFor(service);

        /// <inheritdoc/>
        public IEnumerable<ServiceRegistration> ServiceRegistrationsFor(Service service)
            => _registeredServicesTracker.ServiceRegistrationsFor(service);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _registeredServicesTracker.Dispose();

            base.Dispose(disposing);
        }
    }
}
