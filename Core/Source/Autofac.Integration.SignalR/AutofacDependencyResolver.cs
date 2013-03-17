// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
using Microsoft.AspNet.SignalR;

namespace Autofac.Integration.SignalR
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
    /// </summary>
    public class AutofacDependencyResolver : DefaultDependencyResolver
    {
        readonly ILifetimeScope _lifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver" /> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope that services will be resolved from.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="lifetimeScope" /> is <see langword="null" />.
        /// </exception>
        public AutofacDependencyResolver(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null)
                throw new ArgumentNullException("lifetimeScope");

            _lifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Gets the Autofac implementation of the dependency resolver.
        /// </summary>
        public static AutofacDependencyResolver Current
        {
            get { return GlobalHost.DependencyResolver as AutofacDependencyResolver; }
        }

        /// <summary>
        /// Gets the <see cref="ILifetimeScope"/> that was provided to the constructor.
        /// </summary>
        public ILifetimeScope LifetimeScope
        {
            get { return _lifetimeScope; }
        }

        /// <summary>
        /// Get a single instance of a service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The single instance if resolved; otherwise, <c>null</c>.</returns>
        public override object GetService(Type serviceType)
        {
            return _lifetimeScope.ResolveOptional(serviceType) ?? base.GetService(serviceType);
        }

        /// <summary>
        /// Gets all available instances of a services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The list of instances if any were resolved; otherwise, an empty list.</returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = (IEnumerable<object>)_lifetimeScope.Resolve(enumerableServiceType);
            return instance.Any() ? instance : base.GetServices(serviceType);
        }
    }
}
