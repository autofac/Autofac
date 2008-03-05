// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using Autofac.Component;
using Autofac.Component.Activation;

namespace Autofac.Registrars.Delegate
{
    /// <summary>
    /// Register a component using a delegate.
    /// </summary>
    class DelegateRegistrar : ConcreteRegistrar<IConcreteRegistrar>, IConcreteRegistrar
	{
        ComponentActivator _creator;

        /// <summary>
        /// Initializes a new instance of the DelegateRegistrar&lt;TComponent&gt; class.
        /// </summary>
        /// <param name="implementor">The implementor.</param>
        /// <param name="creator">The creator.</param>
        public DelegateRegistrar(Type implementor, ComponentActivator creator)
			: base(implementor)
		{
            Enforce.ArgumentNotNull(creator, "creator");
            _creator = creator;
		}

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected override IActivator CreateActivator()
        {
            return new DelegateActivator(_creator);
        }

        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override IConcreteRegistrar Syntax
        {
            get { return this; }
        }
    }
}
