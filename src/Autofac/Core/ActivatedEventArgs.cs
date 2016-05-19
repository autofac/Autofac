// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
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
        public ActivatedEventArgs(IComponentContext context, IComponentRegistration component, IEnumerable<Parameter> parameters, T instance)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            Context = context;
            Component = component;
            Parameters = parameters;
            Instance = instance;
        }

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
