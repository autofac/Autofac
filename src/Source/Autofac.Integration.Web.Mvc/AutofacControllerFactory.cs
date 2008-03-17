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
using System.Web.Mvc;
using System.Web.Routing;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// An MS-MVC controller factory that returns controllers built by an
    /// Autofac IoC container scoped according to the current request.
    /// </summary>
    public class AutofacControllerFactory : IControllerFactory
    {
        IContainerProvider _containerProvider;
        IControllerIdentificationStrategy _controllerIdentificationStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacControllerFactory"/> class.
        /// </summary>
        /// <param name="containerProvider">The container provider.</param>
        public AutofacControllerFactory(IContainerProvider containerProvider)
            : this(containerProvider, new DefaultControllerIdentificationStrategy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacControllerFactory"/> class.
        /// </summary>
        /// <param name="containerProvider">The container provider.</param>
        /// <param name="controllerIdentificationStrategy">The controller identification strategy.</param>
        public AutofacControllerFactory(
            IContainerProvider containerProvider, 
            IControllerIdentificationStrategy controllerIdentificationStrategy)
        {
            if (containerProvider == null)
                throw new ArgumentNullException("containerProvider");

            if (controllerIdentificationStrategy == null)
                throw new ArgumentNullException("controllerIdentificationStrategy");

            _containerProvider = containerProvider;
            _controllerIdentificationStrategy = controllerIdentificationStrategy;
        }

        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>The controller.</returns>
        public virtual IController CreateController(RequestContext context, string controllerName)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (controllerName == null)
                throw new ArgumentNullException("controllerName");

            return (IController)_containerProvider.RequestContainer.Resolve(
                _controllerIdentificationStrategy.ServiceForControllerName(controllerName));
        }

        /// <summary>
        /// Disposes the controller. Unecessary in an Autofac-managed application.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public virtual void DisposeController(IController controller)
        {
        }
    }
}
