// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Core
{
    /// <summary>
    /// Fired before the activation process to allow parameters to be changed or an alternative
    /// instance to be provided.
    /// </summary>
    public class PreparingEventArgs : EventArgs
    {
        private IEnumerable<Parameter> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparingEventArgs"/> class.
        /// </summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="context">The context.</param>
        /// <param name="component">The component.</param>
        /// <param name="parameters">The parameters.</param>
        public PreparingEventArgs(IComponentContext context, Service service, IComponentRegistration component, IEnumerable<Parameter> parameters)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Service = service;
            Component = component ?? throw new ArgumentNullException(nameof(component));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        public Service Service { get; }

        /// <summary>
        /// Gets the context in which the activation is occurring.
        /// </summary>
        public IComponentContext Context { get; }

        /// <summary>
        /// Gets the component providing the instance being activated.
        /// </summary>
        public IComponentRegistration Component { get; }

        /// <summary>
        /// Gets or sets the parameters supplied to the activator.
        /// </summary>
        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
