// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Autofac.Diagnostics;
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Standard container implementation.
    /// </summary>
    [DebuggerDisplay("Tag = {Tag}, IsDisposed = {IsDisposed}")]
    public class Container : Disposable, IContainer, IServiceProvider
    {
        private readonly LifetimeScope _rootLifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="componentRegistry">The registry of components.</param>
        internal Container(IComponentRegistry componentRegistry)
        {
            ComponentRegistry = componentRegistry;
            _rootLifetimeScope = new LifetimeScope(ComponentRegistry);
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
        /// that adds component registrations visible only in the new scope.</param>
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
        /// that adds component registrations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return _rootLifetimeScope.BeginLifetimeScope(tag, configurationAction);
        }

        /// <inheritdoc/>
        public DiagnosticListener DiagnosticSource => _rootLifetimeScope.DiagnosticSource;

        /// <summary>
        /// Gets the disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        public IDisposer Disposer => _rootLifetimeScope.Disposer;

        /// <summary>
        /// Gets the tag applied to the lifetime scope.
        /// </summary>
        /// <remarks>The tag applied to this scope and the contexts generated when
        /// it resolves component dependencies.</remarks>
        public object Tag => _rootLifetimeScope.Tag;

        /// <summary>
        /// Fired when a new scope based on the current scope is beginning.
        /// </summary>
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
        {
            add { _rootLifetimeScope.ChildLifetimeScopeBeginning += value; }
            remove { _rootLifetimeScope.ChildLifetimeScopeBeginning -= value; }
        }

        /// <summary>
        /// Fired when this scope is ending.
        /// </summary>
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
        {
            add { _rootLifetimeScope.CurrentScopeEnding += value; }
            remove { _rootLifetimeScope.CurrentScopeEnding -= value; }
        }

        /// <summary>
        /// Fired when a resolve operation is beginning in this scope.
        /// </summary>
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
        {
            add { _rootLifetimeScope.ResolveOperationBeginning += value; }
            remove { _rootLifetimeScope.ResolveOperationBeginning -= value; }
        }

        /// <summary>
        /// Gets associated services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry { get; }

        /// <inheritdoc />
        public object ResolveComponent(ResolveRequest request)
        {
            return _rootLifetimeScope.ResolveComponent(request);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rootLifetimeScope.Dispose();
                ComponentRegistry.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await _rootLifetimeScope.DisposeAsync().ConfigureAwait(false);

                // Registries are not likely to have async tasks to dispose of,
                // so we will leave it as a straight dispose.
                ComponentRegistry.Dispose();
            }

            // Do not call the base, otherwise the standard Dispose will fire.
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
        public object GetService(Type serviceType)
        {
            // GetService implementation on LifetimeScope either returns an object, or throws.
            return ((IServiceProvider)_rootLifetimeScope).GetService(serviceType)!;
        }
    }
}
