// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Core
{
    /// <summary>
    /// Allows registrations to be made on-the-fly when unregistered
    /// services are requested (lazy registrations.)
    /// </summary>
    public interface IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        /// <remarks>
        /// If the source is queried for service s, and it returns a component that implements both s and s', then it
        /// will not be queried again for either s or s'. This means that if the source can return other implementations
        /// of s', it should return these, plus the transitive closure of other components implementing their
        /// additional services, along with the implementation of s. It is not an error to return components
        /// that do not implement <paramref name="service"/>.
        /// </remarks>
        IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor);

        /// <summary>
        /// Gets a value indicating whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (e.g., Meta, Func, or Owned).
        /// </summary>
        bool IsAdapterForIndividualComponents { get; }
    }
}
