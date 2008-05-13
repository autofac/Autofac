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
using System.Globalization;
using System.Linq;
using Autofac.Component;
using Autofac.Component.Scope;

namespace Autofac.Registrars
{
    /// <summary>
    /// Registers a regular component.
    /// </summary>
    public abstract class ConcreteRegistrar<TSyntax> : Registrar<TSyntax>, IModule, IConcreteRegistrar<TSyntax>
        where TSyntax : IConcreteRegistrar<TSyntax>
    {
        Type _implementor;
        Service _defaultService;
        Service _id = new UniqueService();

        /// <summary>
        /// Initializes a new instance of the ComponentRegistrar&lt;TComponent&gt; class.
        /// </summary>
        /// <param name="implementor">The implementation type.</param>
        protected ConcreteRegistrar(Type implementor)
            : this(implementor, new TypedService(Enforce.ArgumentNotNull(implementor, "implementor")))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ComponentRegistrar&lt;TComponent&gt; class.
        /// </summary>
        /// <param name="implementor">The implementation type.</param>
        /// <param name="defaultService">The default service.</param>
        protected ConcreteRegistrar(Type implementor, Service defaultService)
        {
            _implementor = Enforce.ArgumentNotNull(implementor, "implementor");
            _defaultService = Enforce.ArgumentNotNull(defaultService, "defaultService");
        }

		#region IModule Members

        /// <summary>
        /// Registers the component. If the component has not been assigned a name, explicit
        /// services or a factory delegate, then it will be registered as providing its own type
        /// as the default service.
        /// </summary>
        /// <param name="container">The container.</param>
		protected override void DoConfigure(IContainer container)
		{
            Enforce.ArgumentNotNull(container, "container");

            var services = new List<Service>(Services);

            if (services.Count == 0)
                services.Add(_defaultService);
            
            var activator = CreateActivator();
            Enforce.NotNull(activator);

            var descriptor = new Descriptor(Id, services, _implementor, ExtendedProperties);
            var cr = RegistrationCreator(descriptor, activator, Scope.ToIScope(), Ownership);

            RegisterComponent(container, cr);
		}

 		#endregion

        /// <summary>
        /// Setst the name of the registration.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public TSyntax Named(string name)
        {
            AddService(new NamedService(Enforce.ArgumentNotNullOrEmpty(name, "name")));
            return Syntax;
        }

        /// <summary>
        /// Change the service associated with the registration.
        /// </summary>
        /// <param name="services">The services that the registration will expose.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public TSyntax As(params Service[] services)
        {
            Enforce.ArgumentNotNull(services, "services");
            AddServices(services);
            return Syntax;
        }

        /// <summary>
        /// The services exposed by this registration.
        /// </summary>
        /// <value></value>
		protected override IEnumerable<Service> Services
		{
			get
			{
				return base.Services;
			}
		}

        /// <summary>
        /// Add a service to be exposed by the component.
        /// </summary>
        /// <param name="service">The service to add.</param>
        protected override void AddService(Service service)
        {
            Enforce.ArgumentNotNull(service, "service");

            TypedService serviceType = service as TypedService;
            if (serviceType != null && !serviceType.ServiceType.IsAssignableFrom(_implementor))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    ConcreteRegistrarResources.ComponentDoesNotSupportService, _implementor, service));

            base.AddService(service);
        }

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected abstract IActivator CreateActivator();
        
        /// <summary>
        /// A unique service identifier that will be associated with the resulting
        /// registration.
        /// </summary>
        /// <remarks>Only created if accessed.</remarks>
        public Service Id
        {
        	get
        	{
        		return _id;
        	}
        }

        /// <summary>
        /// Filters the services exposed by the registration to include only those that are
        /// not already registered. I.e., will not override existing registrations.
        /// </summary>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public virtual TSyntax DefaultOnly()
        {
            IContainer container = null;

            // First, get a reference to the container. This will be called before
            // the registration creator.
            OnlyIf(c => { container = c; return true; });

            // Then, filter the services when creating the registration:
            var rc = RegistrationCreator;
            RegistrationCreator = (descriptor, activator, scope, ownership) =>
                rc(FilterRegisteredServices(descriptor, container), activator, scope, ownership);

            return Syntax;
        }

        IComponentDescriptor FilterRegisteredServices(IComponentDescriptor descriptor, IContainer container)
        {
            Enforce.ArgumentNotNull(descriptor, "descriptor");
            Enforce.ArgumentNotNull(container, "container");

            var filteredServices = descriptor.Services.Where(s => !container.IsRegistered(s));
            return new Descriptor(descriptor.Id, filteredServices, descriptor.BestKnownImplementationType, descriptor.ExtendedProperties);
        }
	}
}
