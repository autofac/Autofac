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

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// An <see cref="IHttpModule"/> and <see cref="ILifetimeScopeProvider"/> implementation 
    /// that creates a nested lifetime scope for each HTTP request.
    /// </summary>
    internal class RequestLifetimeHttpModule : IHttpModule, ILifetimeScopeProvider
    {
        /// <summary>
        /// Tag used to identify registrations that are scoped to the HTTP request level.
        /// </summary>
        internal static readonly object HttpRequestTag = "httpRequest";

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the 
        /// methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.EndRequest += ContextEndRequest;
        }

        /// <summary>
        /// Gets a nested lifetime scope that services can be resolved from.
        /// </summary>
        /// <param name="container">The parent container.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in nested lifetime scopes.</param>
        /// <returns>A new or existing nested lifetime scope.</returns>
        public ILifetimeScope GetLifetimeScope(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
        {
            return LifetimeScope ?? (LifetimeScope = InitializeLifetimeScope(configurationAction, container));
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        static ILifetimeScope LifetimeScope
        {
            get { return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)]; }
            set { HttpContext.Current.Items[typeof(ILifetimeScope)] = value; }
        }

        static void ContextEndRequest(object sender, EventArgs e)
        {
            ILifetimeScope lifetimeScope = LifetimeScope;
            if (lifetimeScope != null)
                lifetimeScope.Dispose();
        }

        static ILifetimeScope InitializeLifetimeScope(Action<ContainerBuilder> configurationAction, ILifetimeScope container)
        {
            return (configurationAction == null)
                ? container.BeginLifetimeScope(HttpRequestTag)
                : container.BeginLifetimeScope(HttpRequestTag, configurationAction);
        }
    }
}