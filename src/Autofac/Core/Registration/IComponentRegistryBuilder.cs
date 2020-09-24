// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Used to build a <see cref="IComponentRegistry" />.
    /// </summary>
    public interface IComponentRegistryBuilder : IDisposable
    {
        /// <summary>
        /// Create a new <see cref="IComponentRegistry" /> with all the component registrations that have been made.
        /// </summary>
        /// <returns>A new component registry with the configured component registrations.</returns>
        IComponentRegistry Build();

        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        IDictionary<string, object?> Properties { get; }

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
        /// Fired whenever a component is registered - either explicitly or via a
        /// <see cref="IRegistrationSource"/>.
        /// </summary>
        event EventHandler<ComponentRegisteredEventArgs> Registered;

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service to test.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(Service service);

        /// <summary>
        /// Add a registration source that will provide registrations on-the-fly.
        /// </summary>
        /// <param name="source">The source to register.</param>
        void AddRegistrationSource(IRegistrationSource source);

        /// <summary>
        /// Add a source of service middleware that will provide service registrations on-the-fly.
        /// </summary>
        /// <param name="serviceMiddlewareSource">The source to add.</param>
        void AddServiceMiddlewareSource(IServiceMiddlewareSource serviceMiddlewareSource);

        /// <summary>
        /// Register a piece of service middleware that will be invoked for all registrations of a service when they are resolved.
        /// </summary>
        /// <param name="service">The service to register middleware for.</param>
        /// <param name="middleware">The middleware to register.</param>
        /// <param name="insertionMode">The mode of insertion into the pipeline.</param>
        void RegisterServiceMiddleware(Service service, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase);

        /// <summary>
        /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
        /// </summary>
        event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;
    }
}
