// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Pulls registrations from another component registry.
    /// Excludes most auto-generated registrations - currently has issues with
    /// collection registrations.
    /// </summary>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The creator of the component registry is responsible for disposal.")]
    internal class ExternalRegistrySource : IRegistrationSource
    {
        private readonly IComponentRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalRegistrySource"/> class.
        /// </summary>
        /// <param name="registry">Component registry to pull registrations from.</param>
        public ExternalRegistrySource(IComponentRegistry registry)
            => _registry = registry ?? throw new ArgumentNullException(nameof(registry));

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            // Issue #475: This method was refactored significantly to handle
            // registrations made on the fly in parent lifetime scopes to correctly
            // pass to child lifetime scopes.

            // Issue #272: Taking from the registry the following registrations:
            //   - non-adapting own registrations: wrap them with ExternalComponentRegistration
            foreach (var registration in _registry.RegistrationsFor(service))
            {
                if (registration is ExternalComponentRegistration || !registration.IsAdapting())
                {
                    yield return new ExternalComponentRegistration(service, registration);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether components are adapted from the same logical scope.
        /// In this case because the components that are adapted do not come from the same
        /// logical scope, we must return false to avoid duplicating them.
        /// </summary>
        public bool IsAdapterForIndividualComponents => false;
    }
}
