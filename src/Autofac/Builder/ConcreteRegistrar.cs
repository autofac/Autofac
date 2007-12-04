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
using System.Collections.Generic;
using System.Globalization;
using Autofac.Component;
using Autofac.Component.Registration;
using Autofac.Component.Scope;

namespace Autofac.Builder
{
    /// <summary>
    /// Registers a regular component.
    /// </summary>
    abstract class ConcreteRegistrar<TSyntax> : Registrar<TSyntax>, IModule, IConcreteRegistrar<TSyntax>
        where TSyntax : IConcreteRegistrar<TSyntax>
    {
        Type _implementor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistrar&lt;TComponent&gt;"/> class.
        /// </summary>
        /// <param name="implementor">The implementation type.</param>
		protected ConcreteRegistrar(Type implementor)
		{
            Enforce.ArgumentNotNull(implementor, "implementor");
            _implementor = implementor;
		}

		#region IModule Members

        /// <summary>
        /// Registers the component. If the component has not been assigned a name, explicit
        /// services or a factory delegate, then it will be registered as providing its own type
        /// as the default service.
        /// </summary>
        /// <param name="container">The container.</param>
		public void Configure(Container container)
		{
            Enforce.ArgumentNotNull(container, "container");

            var services = new List<Service>(Services);

            Service factoryId = new UniqueService();
            if (FactoryDelegates.Count != 0)
                services.Add(factoryId);

            if (services.Count == 0)
                services.Add(new TypedService(_implementor));

            var activator = CreateActivator();
            Enforce.NotNull(activator);

            var cr = new ComponentRegistration(services, activator, Scope.ToIScope(), Ownership);

            foreach (var activatingHandler in ActivatingHandlers)
                cr.Activating += activatingHandler;

            foreach (var activatedHandler in ActivatedHandlers)
                cr.Activated += activatedHandler;

            container.RegisterComponent(cr);

            foreach (Type factoryDelegate in FactoryDelegates)
            {
                var fr = new ContextAwareDelegateRegistration(factoryDelegate, (c, p) =>
                        {
                            object instance;
                            if (!c.TryResolve(Enforce.NotNull(factoryId), out instance, MakeNamedValues(p)))
                                throw new ComponentNotRegisteredException(factoryId);
                            return instance;
                        });
                container.RegisterComponent(fr);
            }

            FireRegistered(container);
		}

		#endregion

        private Parameter[] MakeNamedValues(IActivationParameters p)
        {
            Enforce.ArgumentNotNull(p, "p");
            var result = new Parameter[p.Count];
            int next = 0;
            foreach (KeyValuePair<string, object> pval in p)
                result[next++] = new Parameter(pval.Key, pval.Value);
            return result;
        }

        /// <summary>
        /// Setst the name of the registration.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TSyntax Named(string name)
        {
            AddService(new NamedService(Enforce.ArgumentNotNullOrEmpty(name, "name")));
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
        /// <param name="service"></param>
        protected override void AddService(Service service)
        {
            Enforce.ArgumentNotNull(service, "service");

            TypedService serviceType = service as TypedService;
            if (serviceType != null && !serviceType.ServiceType.IsAssignableFrom(_implementor))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    ComponentRegistrarResources.ComponentDoesNotSupportService, _implementor, service));

            base.AddService(service);
        }

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected abstract IActivator CreateActivator();
	}
}
