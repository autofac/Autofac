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
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Builder;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Registers 
    /// </summary>
    public class AutofacControllerModule : Autofac.Builder.Module
    {
        IEnumerable<Assembly> _controllerAssemblies;
        
        IControllerIdentificationStrategy _identificationStrategy =
            new DefaultControllerIdentificationStrategy();

        EventHandler<PreparingEventArgs> _preparingHandler;
        EventHandler<ActivatingEventArgs> _activatingHandler;
        EventHandler<ActivatedEventArgs> _activatedHandler;

		Type _actionInvokerType;
        Service _actionInvokerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacControllerModule"/> class.
        /// </summary>
        /// <param name="controllerAssemblies">The controller assemblies.</param>
        public AutofacControllerModule(params Assembly[] controllerAssemblies)
        {
            if (controllerAssemblies == null)
                throw new ArgumentNullException("controllerAssemblies");

            _controllerAssemblies = controllerAssemblies;
        }

        /// <summary>
        /// Gets or sets the identification strategy.
        /// </summary>
        /// <value>The identification strategy.</value>
        public IControllerIdentificationStrategy IdentificationStrategy
        {
            get
            {
                return _identificationStrategy;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _identificationStrategy = value;
            }
        }

        /// <summary>
        /// Sets an event handler that will be called when controller instances
        /// are requested.
        /// </summary>
        public EventHandler<PreparingEventArgs> PreparingHandler
        {
            get
            {
                return _preparingHandler;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _preparingHandler = value;
            }
        }

        /// <summary>
        /// Sets an event handler that will be called when controller instances
        /// are being activated.
        /// </summary>
        public EventHandler<ActivatingEventArgs> ActivatingHandler
        {
            get
            {
                return _activatingHandler;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _activatingHandler = value;
            }
        }

        /// <summary>
        /// Sets an event handler that will be called when controller instances
        /// are activated.
        /// </summary>
        public EventHandler<ActivatedEventArgs> ActivatedHandler
        {
            get
            {
                return _activatedHandler;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _activatedHandler = value;
            }
        }

        /// <summary>
        /// Specify a type that will be used as the action invoker for discovered controllers.
        /// The type will be registered in the container.
        /// To control the registration of the action invoker, use <see cref="ActionInvokerService"/> instead.
        /// </summary>
        public Type ActionInvokerType
        {
            get
            {
                return _actionInvokerType;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _actionInvokerType = value;
            }
        }

        /// <summary>
        /// Specify a service that will be used as the action invoker for discovered controllers.
        /// The service must already exist in the container.
        /// </summary>
        public Service ActionInvokerService
        {
            get
            {
                return _actionInvokerService;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _actionInvokerService = value;
            }
        }

        /// <summary>
        /// Adds registrations to the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            if (builder == null)
                throw new ArgumentNullException("builder");

            if (_actionInvokerType != null && _actionInvokerService != null)
                throw new InvalidOperationException(AutofacControllerModuleResources.SpecifyOnlyOneActionInvoker);

            if (_actionInvokerType != null)
            {
                _actionInvokerService = new UniqueService();
                builder.Register(_actionInvokerType).As(_actionInvokerService).FactoryScoped();
            }

            var controllerTypes = from assembly in _controllerAssemblies
                                  from type in assembly.GetTypes()
                                  where typeof(IController).IsAssignableFrom(type) &&
                                    !type.IsAbstract
                                  select type;

            foreach (var controllerType in controllerTypes)
            {
                var registration = builder.Register(controllerType)
                    .FactoryScoped()
                    .As(IdentificationStrategy.ServiceForControllerType(controllerType));

                if (_actionInvokerService != null)
                    registration.OnActivating(InjectActionInvoker);

                if (_preparingHandler != null)
                    registration.OnPreparing(_preparingHandler);

                if (_activatingHandler != null)
                    registration.OnActivating(_activatingHandler);

                if (_activatedHandler != null)
                    registration.OnActivated(_activatedHandler);
            }
        }
    	
        void InjectActionInvoker(object sender, ActivatingEventArgs e)
    	{
			if(e.Instance is Controller)
			{
				((Controller)e.Instance).ActionInvoker =
                    (IActionInvoker)e.Context.Resolve(_actionInvokerService);
			}
    	}
    }
}
