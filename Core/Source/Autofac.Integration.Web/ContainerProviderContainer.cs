// Contributed by Chad Lee 2009-06-15
// Copyright © 2011 Autofac Contributors
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
using System.Diagnostics.CodeAnalysis;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides an implementation of <see cref="Autofac.IContainer"/> which uses the configured
    /// <see cref="IContainerProvider"/> to route calls to the current request container.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Disposing of this wrapper container should not result in the whole application container being disposed.")]
    public class ContainerProviderContainer : IContainer
    {
        private readonly IContainerProvider _containerProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="Autofac.Integration.Web.ContainerProviderContainer"/>.
        /// </summary>
        /// <param name="containerProvider">The <see cref="IContainerProvider"/> to use to retrieve the current request container.</param>
        public ContainerProviderContainer(IContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException("containerProvider");

            _containerProvider = containerProvider;
        }

        /// <summary>
        /// Associates services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry
        {
            get
            {
                return _containerProvider.RequestLifetime.ComponentRegistry;
            }
        }

        /// <summary>
        /// Begin a new nested scope. Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope();
        }

        /// <summary>
        /// Begin a new nested scope. Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(tag);
        }

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <remarks>
        /// The components registered in the sub-scope will be treated as though they were
        /// registered in the root scope, i.e., SingleInstance() components will live as long
        /// as the root scope.
        /// </remarks>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(configurationAction);
        }

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <remarks>
        /// The components registered in the sub-scope will be treated as though they were
        /// registered in the root scope, i.e., SingleInstance() components will live as long
        /// as the root scope.
        /// </remarks>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(tag, configurationAction);
        }

        /// <summary>
        /// The disposer associated with this <see cref="ILifetimeScope"/>.
        /// Component instances can be associated with it manually if required.
        /// </summary>
        /// <remarks>Typical usage does not require interaction with this member- it
        /// is used when extending the container.</remarks>
        public IDisposer Disposer
        {
            get { return _containerProvider.RequestLifetime.Disposer; }
        }

        /// <summary>
        /// The tag applied to the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <remarks>Tags allow a level in the lifetime hierarchy to be identified.
        /// In most applications, tags are not necessary.</remarks>
        /// <seealso cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope"/>
        public object Tag
        {
            get
            {
                return _containerProvider.RequestLifetime.Tag;
            }
        }

        /// <summary>
        /// Fired when a new scope based on the current scope is beginning.
        /// </summary>
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
        {
            add { _containerProvider.RequestLifetime.ChildLifetimeScopeBeginning += value; }
            remove { _containerProvider.RequestLifetime.ChildLifetimeScopeBeginning -= value; }
        }

        /// <summary>
        /// Fired when this scope is ending.
        /// </summary>
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
        {
            add { _containerProvider.RequestLifetime.CurrentScopeEnding += value; }
            remove { _containerProvider.RequestLifetime.CurrentScopeEnding -= value; }
        }

        /// <summary>
        /// Fired when a resolve operation is beginning in this scope.
        /// </summary>
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
        {
            add { _containerProvider.RequestLifetime.ResolveOperationBeginning += value; }
            remove { _containerProvider.RequestLifetime.ResolveOperationBeginning -= value; }
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
        /// <exception cref="Autofac.Core.DependencyResolutionException"/>
        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _containerProvider.RequestLifetime.ResolveComponent(registration, parameters);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Disposing of this wrapper container should not result in the whole application container being disposed.")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Disposing of this wrapper container should not result in the whole application container being disposed.")]
        public void Dispose()
        {
        }
    }
}