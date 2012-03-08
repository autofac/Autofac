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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Services;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
    /// </summary>
    public class AutofacWebApiDependencyResolver : IDependencyResolver
    {
        readonly ILifetimeScope _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        public AutofacWebApiDependencyResolver(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">Type of service to request.</param>
        /// <returns>An instance of the service, or null if the service is not found.</returns>
        public object GetService(Type serviceType)
        {
            return _container.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">Type of services to request.</param>
        /// <returns>An enumeration (possibly empty) of the service.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = _container.Resolve(enumerableServiceType);
            return (IEnumerable<object>)instance;
        }
    }
}