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
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="IHttpControllerActivator"/> interface.
    /// </summary>
    public class AutofacControllerActivator : IHttpControllerActivator
    {
        /// <summary>
        /// Creates the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>A controller instance if resolved; otherwise, <c>null</c>.</returns>
        public IHttpController Create(HttpControllerContext controllerContext, Type controllerType)
        {
            var requestProperties = controllerContext.Request.Properties;

            if (!requestProperties.ContainsKey(AutofacControllerFactory.ApiRequestTag))
                throw GetInvalidOperationException();

            ILifetimeScope lifetimeScope = requestProperties[AutofacControllerFactory.ApiRequestTag] as ILifetimeScope;
            if (lifetimeScope == null)
                throw GetInvalidOperationException();

            return lifetimeScope.ResolveOptional(controllerType) as IHttpController;
        }

        internal static InvalidOperationException GetInvalidOperationException()
        {
            return new InvalidOperationException(
                string.Format(AutofacControllerActivatorResources.LifetimeScopeMissing,
                    typeof(ILifetimeScope).FullName, typeof(HttpRequestMessage).FullName, typeof(AutofacControllerFactory).FullName));
        }
    }
}