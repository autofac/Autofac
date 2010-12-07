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
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
    /// </summary>
    public class AutofacDependencyResolver : IDependencyResolver
    {
        readonly ILifetimeScope _container;
        readonly Action<ContainerBuilder> _configurationAction;
        ILifetimeScopeProvider _lifetimeScopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public AutofacDependencyResolver(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
            _container.TryResolve(out _lifetimeScopeProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in nested lifetime scopes.</param>
        public AutofacDependencyResolver(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
            : this(container)
        {
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");
            _configurationAction = configurationAction;
        }

        /// <summary>
        /// Get a single instance of a service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The single instance if resolved; otherwise, <c>null</c>.</returns>
        public object GetService(Type serviceType)
        {
            return RequestLifetimeScope.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Gets all available instances of a services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The list of instances if any were resolved; otherwise, an empty list.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = RequestLifetimeScope.Resolve(enumerableServiceType);
#if NET35
            return ((IEnumerable)instance).Cast<object>();
#else
            return (IEnumerable<object>)instance;
#endif
        }

        /// <summary>
        /// The lifetime containing components for processing the current HTTP request.
        /// </summary>
        public ILifetimeScope RequestLifetimeScope
        {
            get
            {
                if (_lifetimeScopeProvider == null)
                {
                    var httpContext = (HttpContext.Current == null) ? null : new HttpContextWrapper(HttpContext.Current);
                    _lifetimeScopeProvider = GetRequestLifetimeHttpModule(httpContext);
                }
                return _lifetimeScopeProvider.GetLifetimeScope(_container, _configurationAction);
            }
        }

        /// <summary>
        /// Gets the request lifetime HTTP module.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The HTTP module as an <see cref="ILifetimeScopeProvider"/> instance.</returns>
        internal static ILifetimeScopeProvider GetRequestLifetimeHttpModule(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new InvalidOperationException(AutofacDependencyResolverResources.HttpContextNotAvailable);

            if (httpContext.ApplicationInstance == null)
                throw new InvalidOperationException(AutofacDependencyResolverResources.ApplicationInstanceNotAvailable);

            var httpModules = httpContext.ApplicationInstance.Modules;
            for (var index = 0; index < httpModules.Count; index++)
            {
                if (httpModules[index] is RequestLifetimeHttpModule)
                    return (RequestLifetimeHttpModule)httpModules[index];
            }
            throw new InvalidOperationException(string.Format(
                AutofacDependencyResolverResources.HttpModuleNotLoaded, typeof(RequestLifetimeHttpModule)));
        }
    }
}
