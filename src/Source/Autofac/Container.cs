// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Core.Activators.Delegate;
using Autofac.Lifetime;
using Autofac.Registration;
using Autofac.SelfRegistration;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Standard container implementation.
    /// </summary>
    public class Container : Disposable, IContainer, ILifetimeScope, IDisposable, IServiceProvider
    {
        readonly IComponentRegistry _componentRegistry;

        readonly ILifetimeScope _rootLifetimeScope;

        /// <summary>
        /// An empty container.
        /// </summary>
        public static readonly Container Empty = new Container();

        /// <summary>
        /// Create a new container.
        /// </summary>
        public Container()
        {
            _componentRegistry = new ComponentRegistry();

            // Lots of ugly cruft around self-registration, needs some refactoring but
            // not sure of the right approach yet

            _componentRegistry.Register(new ComponentRegistration(
                Guid.NewGuid(),
                new DelegateActivator(typeof(IndirectReference<ILifetimeScope>), (c, p) => new IndirectReference<ILifetimeScope>()),
                new CurrentScopeLifetime(),
                InstanceSharing.Shared,
                InstanceOwnership.ExternallyOwned,
                new Service[] { new TypedService(typeof(IndirectReference<ILifetimeScope>)) },
                new Dictionary<string, object>()));

            _componentRegistry.Register(new ComponentRegistration(
                Guid.NewGuid(),
                new DelegateActivator(typeof(LifetimeScope), (c, p) => c.Resolve<IndirectReference<ILifetimeScope>>().Value),
                new CurrentScopeLifetime(),
                InstanceSharing.Shared,
                InstanceOwnership.ExternallyOwned,
                new Service[] { new TypedService(typeof(ILifetimeScope)), new TypedService(typeof(IComponentContext)) },
                new Dictionary<string, object>()));

            _rootLifetimeScope = new LifetimeScope(_componentRegistry);
            _rootLifetimeScope.Resolve<IndirectReference<ILifetimeScope>>().Value = _rootLifetimeScope;
        }

        /// <summary>
        /// Begin a new sub-scope. Instances created via the sub-scope
        /// will be disposed along with it.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return _rootLifetimeScope.BeginLifetimeScope();
        }

        /// <summary>
        /// The disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        public IDisposer Disposer
        {
            get { return _rootLifetimeScope.Disposer; }
        }

        /// <summary>
        /// Tag applied to the lifetime scope.
        /// </summary>
        /// <remarks>The tag applied to this scope and the contexts generated when
        /// it resolves component dependencies.</remarks>
        public object Tag
        {
            get { return _rootLifetimeScope.Tag; }
            set { _rootLifetimeScope.Tag = value; }
        }

        /// <summary>
        /// Associates services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry
        {
            get { return _componentRegistry; }
        }

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _rootLifetimeScope.Resolve(registration, parameters);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rootLifetimeScope.Dispose();
                _componentRegistry.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object 
        /// to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is 
        /// no service object of type <paramref name="serviceType"/>.
        /// </returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IServiceProvider)_rootLifetimeScope).GetService(serviceType);
        }
    }
}
