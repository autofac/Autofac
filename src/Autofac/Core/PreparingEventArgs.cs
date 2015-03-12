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
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Fired before the activation process to allow parameters to be changed or an alternative
    /// instance to be provided.
    /// </summary>
    public class PreparingEventArgs : EventArgs
    {
        readonly IComponentContext _context;
        readonly IComponentRegistration _component;
        IEnumerable<Parameter> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparingEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="component">The component.</param>
        /// <param name="parameters">The parameters.</param>
        public PreparingEventArgs(IComponentContext context, IComponentRegistration component, IEnumerable<Parameter> parameters)
        {
            _context = Enforce.ArgumentNotNull(context, "context");
            _component = Enforce.ArgumentNotNull(component, "component");
            _parameters = Enforce.ArgumentNotNull(parameters, "parameters");
        }

        /// <summary>
        /// The context in which the activation is occurring.
        /// </summary>
        public IComponentContext Context
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// The component providing the instance being activated.
        /// </summary>
        public IComponentRegistration Component
        {
            get
            {
                return _component;
            }
        }

        /// <summary>
        /// The parameters supplied to the activator.
        /// </summary>
        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _parameters = value;
            }
        }
    }
}
