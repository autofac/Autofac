// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Autofac.Integration.Web.Mvc
{

    /// <summary>
    /// An MS-MVC controller factory that returns controllers built by an
    /// Autofac IoC container scoped according to the current request.
    /// </summary>
    public class AutofacControllerFactory : DefaultControllerFactory
    {
        readonly IContainerProvider _containerProvider;
        readonly IControllerIdentificationStrategy _controllerIdentificationStrategy;

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

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            return base.CreateController(requestContext, controllerName);
        }

        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>The controller.</returns>
        protected override IController GetControllerInstance(RequestContext context, Type controllerType)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            //
            // a null controller type is a 404, because the base class couldn't resolve the controller name back to a type
            // a common example of this case would be a non-existant favicon.ico
            if (controllerType == null)
            {
                throw new HttpException(404,
                    string.Format("controller type was not found for path {0}",
                        context.HttpContext.Request.Path));
            }

            var controllerService = _controllerIdentificationStrategy
                .ServiceForControllerType(controllerType);

            object controller = null;
            if (_containerProvider.RequestLifetime.TryResolve(controllerService, out controller))
                return (IController)controller;
            else
                throw new HttpException(404,
                    string.Format(AutofacControllerFactoryResources.NotFound,
                        controllerService,
                        controllerType.FullName,
                        context.HttpContext.Request.Path));
        }


        /// <summary>
        /// Releases the controller. Unecessary in an Autofac-managed application
        /// </summary>
        /// <param name="controller">The controller.</param>
        public override void ReleaseController(IController controller)
        {
        }
    }
}
