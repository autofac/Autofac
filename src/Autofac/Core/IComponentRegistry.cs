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
        IDictionary<string, object> Properties { get; }

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
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        void Register(IComponentRegistration registration);

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        /// <param name="preserveDefaults">If true, existing defaults for the services provided by the
        /// component will not be changed.</param>
        void Register(IComponentRegistration registration, bool preserveDefaults);

        /// <summary>
        /// Gets the set of registered components.
        /// </summary>
        IEnumerable<IComponentRegistration> Registrations { get; }

        /// <summary>
        /// Selects from the available registrations after ensuring that any
        /// dynamic registration sources that may provide <paramref name="service"/>
        /// have been invoked.
        /// </summary>
        /// <param name="service">The service for which registrations are sought.</param>
        /// <returns>Registrations supporting <paramref name="service"/>.</returns>
        IEnumerable<IComponentRegistration> RegistrationsFor(Service service);

        /// <summary>
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<ComponentRegisteredEventArgs> Registered;

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void AddRegistrationSource(IRegistrationSource source);

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
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;
    }
}
