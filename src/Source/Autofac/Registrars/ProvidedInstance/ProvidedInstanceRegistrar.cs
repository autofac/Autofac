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

namespace Autofac.Registrars.ProvidedInstance
{
    /// <summary>
    /// Register a component using a provided instance.
    /// </summary>
    class ProvidedInstanceRegistrar : ConcreteRegistrar<IConcreteRegistrar>, IConcreteRegistrar
	{
        object _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidedInstanceRegistrar"/> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public ProvidedInstanceRegistrar(object instance)
            : this(instance, Enforce.ArgumentNotNull(instance, "instance").GetType())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidedInstanceRegistrar"/> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="defaultService">The default service.</param>
        public ProvidedInstanceRegistrar(object instance, Type defaultService)
            : base(defaultService)
        {
        	_instance = Enforce.ArgumentNotNull(instance, "instance");
        }

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected override IActivator CreateActivator()
        {
            return new ProvidedInstanceActivator(_instance);
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
        /// Change the scope associated with the registration.
        /// This determines how instances are tracked and shared.
        /// </summary>
        /// <param name="scope">The scope model to use.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public override IConcreteRegistrar WithScope(InstanceScope scope)
        {
            if (scope != InstanceScope.Singleton)
                throw new ArgumentException(ProvidedInstanceRegistrarResources.SingletonScopeOnly);
            return base.WithScope(scope);
        }

        /// <summary>
        /// Resolves issue #7 - Provided instances not disposed unless they are resolved
        /// first. Manually attaches instance to container's disposer if Container ownership
        /// is chosen.
        /// </summary>
        /// <param name="container">The container.</param>
        public override void Configure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");

            InstanceOwnership ownership = Ownership;
            Ownership = InstanceOwnership.External;

            base.Configure(container);
            
            if (ownership == InstanceOwnership.Container && _instance is IDisposable)
                container.Disposer.AddInstanceForDisposal((IDisposable)_instance);
        }
    }
}
