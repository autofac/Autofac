// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using Autofac.Core;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Registers 
    /// </summary>
    public class AutofacControllerModule : Autofac.Module
    {
        IEnumerable<Assembly> _controllerAssemblies;
        
        IControllerIdentificationStrategy _identificationStrategy =
            new DefaultControllerIdentificationStrategy();

        EventHandler<PreparingEventArgs<object>> _preparingHandler;
        EventHandler<ActivatingEventArgs<object>> _activatingHandler;
        EventHandler<ActivatedEventArgs<object>> _activatedHandler;

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
        public EventHandler<PreparingEventArgs<object>> PreparingHandler
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
        public EventHandler<ActivatingEventArgs<object>> ActivatingHandler
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
        public EventHandler<ActivatedEventArgs<object>> ActivatedHandler
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
                builder.RegisterType(_actionInvokerType).As(_actionInvokerService);
            }

            var controllerTypes = from assembly in _controllerAssemblies
                                  from type in assembly.GetTypes()
                                  where typeof(IController).IsAssignableFrom(type) &&
                                    !type.IsAbstract
                                  select type;

            foreach (var controllerType in controllerTypes)
            {
                var registration = builder.RegisterType(controllerType)
                    .As(IdentificationStrategy.ServiceForControllerType(controllerType));

                if (_actionInvokerService != null)
                    registration.OnActivating(e => InjectActionInvoker((IController)e.Instance, e.Context));

                if (_preparingHandler != null)
                    registration.OnPreparing(e => _preparingHandler(this, e));

                if (_activatingHandler != null)
                    registration.OnActivating(e => _activatingHandler(this, e));

                if (_activatedHandler != null)
                    registration.OnActivated(e => _activatedHandler(this, e));
            }
        }

        void InjectActionInvoker(IController controller, IComponentContext context)
    	{
			if(controller is Controller)
			{
				((Controller)controller).ActionInvoker =
                    (IActionInvoker)context.Resolve(_actionInvokerService);
			}
    	}
    }
}
