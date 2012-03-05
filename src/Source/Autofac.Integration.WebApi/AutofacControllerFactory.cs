// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
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
using System.Collections.Concurrent;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="IHttpControllerFactory"/> interface.
    /// </summary>
    public class AutofacControllerFactory : DefaultHttpControllerFactory
    {
        readonly ILifetimeScope _container;
        readonly ConcurrentDictionary<IHttpController, ILifetimeScope> _controllers = new ConcurrentDictionary<IHttpController, ILifetimeScope>();

        /// <summary>
        /// Tag used to identify registrations that are scoped to the API request level.
        /// </summary>
        internal static readonly string ApiRequestTag = "AutofacApiRequest";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacControllerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration for the <see cref="HttpServer"/> instance.</param>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        public AutofacControllerFactory(HttpConfiguration configuration, ILifetimeScope container) : base(configuration)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>A controller instance if resolved; otherwise, <c>null</c>.</returns>
        public override IHttpController CreateController(HttpControllerContext controllerContext, string controllerName)
        {
            var lifetimeScope = _container.BeginLifetimeScope(ApiRequestTag);
            controllerContext.Request.Properties.Add(ApiRequestTag, lifetimeScope);

            var controller = base.CreateController(controllerContext, controllerName);
            _controllers.TryAdd(controller, lifetimeScope);

            return controller;
        }

        /// <summary>
        /// Releases the controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public override void ReleaseController(IHttpController controller)
        {
            ILifetimeScope lifetimeScope;
            if (_controllers.TryRemove(controller, out lifetimeScope))
                if (lifetimeScope != null)
                    lifetimeScope.Dispose();
        }
    }

}