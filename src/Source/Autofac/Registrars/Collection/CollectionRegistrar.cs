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

using Autofac.Component.Scope;
using System.Linq;

namespace Autofac.Registrars.Collection
{
    /// <summary>
    /// Register a component using a provided instance.
    /// </summary>
    class CollectionRegistrar<TItem> : Registrar<IConcreteRegistrar>, IConcreteRegistrar, IModule
    {
        Service _id; // Default is null.
        
        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override IConcreteRegistrar Syntax
        {
            get { return this; }
        }

        #region IConcreteRegistrar Members

        /// <summary>
        /// Names the registration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IConcreteRegistrar Named(string name)
        {
            Enforce.ArgumentNotNullOrEmpty(name, "name");
            AddService(new NamedService(name));
            return Syntax;
        }

        public IConcreteRegistrar As(params Service[] services)
        {
            Enforce.ArgumentNotNull(services, "services");
            foreach (var service in services)
                AddService(service);
            return Syntax;
        }
        
        /// <summary>
        /// A unique service identifier that will be associated with the resulting
        /// registration.
        /// </summary>
        /// <remarks>Only created if accessed.</remarks>
        public Service Id
        {
        	get
        	{
        		return _id = _id ?? new UniqueService();
        	}
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
            var services = Services;            
            
            if (_id != null)
            	services = services.Concat(new[]{ _id });
            
            container.RegisterComponent(
            	new ServiceListRegistration<TItem>(services, Scope.ToIScope()));
        }

        #endregion
    }
}
