﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
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
                    yield return new ExternalComponentRegistration(
                        Guid.NewGuid(),
                        #pragma warning disable CA2000 // Dispose objects before losing scope
                        new DelegateActivator(registration.Activator.LimitType, (c, p) => c.ResolveComponent(new ResolveRequest(service, registration, p))),
                        #pragma warning restore CA2000 // Dispose objects before losing scope
                        new CurrentScopeLifetime(),
                        InstanceSharing.None,
                        InstanceOwnership.ExternallyOwned,
                        new[] { service },
                        registration.Metadata,
                        registration,
                        false);
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
