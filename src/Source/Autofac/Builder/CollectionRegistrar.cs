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

using Autofac.Component.Activation;
using Autofac.Component;
using Autofac.Component.Registration;
using Autofac.Component.Scope;

namespace Autofac.Builder
{
    /// <summary>
    /// Register a component using a provided instance.
    /// </summary>
    class CollectionRegistrar<TItem> : Registrar<ICollectionRegistrar>, ICollectionRegistrar, IModule
    {
        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override ICollectionRegistrar Syntax
        {
            get { return this; }
        }

        #region IConcreteRegistrar<ICollectionRegistrar> Members

        /// <summary>
        /// Names the registration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ICollectionRegistrar Named(string name)
        {
            Enforce.ArgumentNotNullOrEmpty(name, "name");
            AddService(new NamedService(name));
            return Syntax;
        }

        public ICollectionRegistrar As(params Service[] services)
        {
            Enforce.ArgumentNotNull(services, "services");
            foreach (var service in services)
                AddService(service);
            return Syntax;
        }

        #endregion

        #region IModule Members

        /// <summary>
        /// Apply the module to the container.
        /// </summary>
        /// <param name="container">Container to apply configuration to.</param>
        public void Configure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            container.RegisterComponent(new ServiceListRegistration<TItem>(Services, Scope.ToIScope()));
        }

        #endregion
    }
}
