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

using System.Collections.Generic;
using Autofac.Component;

namespace Autofac.Registrars.Collection
{
    /// <summary>
    /// Register a component using a provided instance.
    /// </summary>
    class CollectionRegistrar<TItem> : ConcreteRegistrar<IConcreteRegistrar>, IConcreteRegistrar, IModule
    {
        /// <summary>
        /// Constructs a CollectionRegistrar.
        /// </summary>
        public CollectionRegistrar()
            : base(ServiceListActivator<TItem>.ImplementationType, new TypedService(typeof(IEnumerable<TItem>)))
        {
            RegistrationCreator = (descriptor, activator, scope, ownership) =>
                new ServiceListRegistration<TItem>(
                    descriptor,
                    (ServiceListActivator<TItem>)activator, // changing this to an interface dependency would make things more flexible
                    scope);
        }
        
        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override IConcreteRegistrar Syntax
        {
            get { return this; }
        }

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected override IActivator CreateActivator()
        {
            return new ServiceListActivator<TItem>();
        }
    }
}
