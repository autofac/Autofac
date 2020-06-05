// This software is part of the Autofac IoC container
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
