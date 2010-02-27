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
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Standard container implementation.
    /// </summary>
    public class Container : Disposable, IContainer, IServiceProvider
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

            _componentRegistry.Register(new ComponentRegistration(
                LifetimeScope.SelfRegistrationId,
                new DelegateActivator(typeof(LifetimeScope), (c, p) => {
                    throw new InvalidOperationException();
                }),
                new CurrentScopeLifetime(),
                InstanceSharing.Shared,
                InstanceOwnership.ExternallyOwned,
                new Service[] { new TypedService(typeof(ILifetimeScope)), new TypedService(typeof(IComponentContext)) },
                new Dictionary<string, object>()));

            _rootLifetimeScope = new LifetimeScope(_componentRegistry);
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
        /// Begin a new sub-scope. Instances created via the sub-scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return _rootLifetimeScope.BeginLifetimeScope(tag);
        }

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return _rootLifetimeScope.BeginLifetimeScope(configurationAction);
        }

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return _rootLifetimeScope.BeginLifetimeScope(tag, configurationAction);
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
