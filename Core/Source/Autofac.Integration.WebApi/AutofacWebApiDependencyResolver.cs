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
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
    /// </summary>
    public class AutofacWebApiDependencyResolver : IDependencyResolver
    {
        private bool _disposed;
        readonly ILifetimeScope _container;
        readonly IDependencyScope _rootDependencyScope;

        /// <summary>
        /// Tag used to identify registrations that are scoped to the API request level.
        /// </summary>
        public static readonly string ApiRequestTag = "AutofacWebRequest";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        public AutofacWebApiDependencyResolver(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");

            _container = container;
            _rootDependencyScope = new AutofacWebApiDependencyScope(container);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
        /// </summary>
        ~AutofacWebApiDependencyResolver()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the root container provided to the dependency resolver.
        /// </summary>
        public ILifetimeScope Container
        {
            get { return _container; }
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">Type of service to request.</param>
        /// <returns>An instance of the service, or null if the service is not found.</returns>
        public object GetService(Type serviceType)
        {
            return _rootDependencyScope.GetService(serviceType);
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">ControllerType of services to request.</param>
        /// <returns>An enumeration (possibly empty) of the service.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _rootDependencyScope.GetServices(serviceType);
        }

        /// <summary>
        /// Starts a resolution scope. Objects which are resolved in the given scope will belong to
        /// that scope, and when the scope is disposed, those objects are returned to the container.
        /// </summary>
        /// <returns>
        /// The dependency scope.
        /// </returns>
        public IDependencyScope BeginScope()
        {
            var lifetimeScope = _container.BeginLifetimeScope(ApiRequestTag);
            return new AutofacWebApiDependencyScope(lifetimeScope);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (_rootDependencyScope != null)
                    {
                        _rootDependencyScope.Dispose();
                    }
                }
                this._disposed = true;
            }
        }
    }
}