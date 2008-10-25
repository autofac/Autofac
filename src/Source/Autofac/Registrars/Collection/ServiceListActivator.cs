// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Component;

namespace Autofac.Registrars.Collection
{
    /// <summary>
    /// Custom activator that maintains a service list and returns instances
    /// of List.
    /// </summary>
    class ServiceListActivator<TItem> : IActivator
    {
        IList<Service> _items = new List<Service>();

        /// <summary>
        /// Gets the implementation type.
        /// </summary>
        public static readonly Type ImplementationType = typeof(TItem[]);

        /// <summary>
        /// Gets the services that will appear in instances of the list.
        /// </summary>
        /// <value>The items.</value>
        public IList<Service> Items
        {
            get
            {
                return _items;
            }
        }

        #region IActivator Members

        /// <summary>
        /// Create a component instance, using container
        /// to resolve the instance's dependencies.
        /// </summary>
        /// <param name="context">The context to use
        /// for dependency resolution.</param>
        /// <param name="parameters">Parameters that can be used in the resolution process.</param>
        /// <returns>
        /// A component instance. Note that while the
        /// returned value need not be created on-the-spot, it must
        /// not be returned more than once by consecutive calls. (Throw
        /// an exception if this is attempted. IActivationScope should
        /// manage singleton semantics.)
        /// </returns>
        public object ActivateInstance(IContext context, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            var instance = new TItem[_items.Count];
            for (int i = 0; i < _items.Count; ++i)
            {
                instance[i] = (TItem)context.Resolve(_items[i]);
            }

            return instance;
        }

        /// <summary>
        /// Not supported as the ServiceListRegistration class overrides
        /// DuplicateForNewContext to avoid this method call.
        /// </summary>
        public bool CanSupportNewContext
        {
            get { throw new InvalidOperationException(); }
        }

        #endregion
    }
}
