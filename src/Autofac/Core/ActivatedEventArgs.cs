// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Core
{
    /// <summary>
    /// Fired when the activation process for a new instance is complete.
    /// </summary>
    public class ActivatedEventArgs<T> : EventArgs, IActivatedEventArgs<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="component">The component.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="service">The service being resolved.</param>
        public ActivatedEventArgs(
            IComponentContext context,
            Service service,
            IComponentRegistration component,
            IEnumerable<Parameter> parameters,
            T instance)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        public Service Service { get; }

        /// <summary>
        /// Gets the context in which the activation occurred.
        /// </summary>
        public IComponentContext Context { get; }

        /// <summary>
        /// Gets the component providing the instance.
        /// </summary>
        public IComponentRegistration Component { get; }

        /// <summary>
        /// Gets the paramters provided when resolved.
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Gets the instance that will be used to satisfy the request.
        /// </summary>
        public T Instance { get; }
    }
}
