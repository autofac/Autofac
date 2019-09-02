﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Builder;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Util;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Lifetime scope implementation.
    /// </summary>
    [DebuggerDisplay("Tag = {Tag}, IsDisposed = {IsDisposed}")]
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The creator of the parent lifetime scope is responsible for disposal.")]
    public class LifetimeScope : Disposable, ISharingLifetimeScope, IServiceProvider
    {
        /// <summary>
        /// Protects shared instances from concurrent access. Other members and the base class are threadsafe.
        /// </summary>
        private readonly object _synchRoot = new object();
        private readonly ConcurrentDictionary<Guid, object> _sharedInstances = new ConcurrentDictionary<Guid, object>();

        internal static Guid SelfRegistrationId { get; } = Guid.NewGuid();

        private static readonly Action<ContainerBuilder> NoConfiguration = b => { };

        /// <summary>
        /// The tag applied to root scopes when no other tag is specified.
        /// </summary>
        public static readonly object RootTag = "root";

        private static object MakeAnonymousTag()
        {
            return new object();
        }

        private LifetimeScope()
        {
            _sharedInstances[SelfRegistrationId] = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScope"/> class.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="componentRegistry">Components used in the scope.</param>
        /// <param name="parent">Parent scope.</param>
        protected LifetimeScope(IComponentRegistry componentRegistry, LifetimeScope parent, object tag)
            : this(componentRegistry, tag)
        {
            ParentLifetimeScope = parent ?? throw new ArgumentNullException(nameof(parent));
            RootLifetimeScope = ParentLifetimeScope.RootLifetimeScope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScope"/> class.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="componentRegistry">Components used in the scope.</param>
        public LifetimeScope(IComponentRegistry componentRegistry, object tag)
            : this()
        {
            ComponentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
            RootLifetimeScope = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScope"/> class.
        /// </summary>
        /// <param name="componentRegistry">Components used in the scope.</param>
        public LifetimeScope(IComponentRegistry componentRegistry)
            : this(componentRegistry, RootTag)
        {
        }

        /// <summary>
        /// Begin a new anonymous sub-scope. Instances created via the sub-scope
        /// will be disposed along with it.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return BeginLifetimeScope(MakeAnonymousTag());
        }

        /// <summary>
        /// Begin a new tagged sub-scope. Instances created via the sub-scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            CheckNotDisposed();

            CheckTagIsUnique(tag);

            var registry = new CopyOnWriteRegistry(ComponentRegistry, () => CreateScopeRestrictedRegistry(tag, NoConfiguration));
            var scope = new LifetimeScope(registry, this, tag);
            scope.Disposer.AddInstanceForDisposal(registry);
            RaiseBeginning(scope);
            return scope;
        }

        private void CheckTagIsUnique(object tag)
        {
            ISharingLifetimeScope parentScope = this;
            while (parentScope != RootLifetimeScope)
            {
                if (parentScope.Tag.Equals(tag))
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, LifetimeScopeResources.DuplicateTagDetected, tag));
                }

                parentScope = parentScope.ParentLifetimeScope;
            }
        }

        [SuppressMessage("CA1030", "CA1030", Justification = "This method raises the event; it's not the event proper.")]
        private void RaiseBeginning(ILifetimeScope scope)
        {
            var handler = ChildLifetimeScopeBeginning;
            handler?.Invoke(this, new LifetimeScopeBeginningEventArgs(scope));
        }

        /// <summary>
        /// Begin a new anonymous sub-scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registrations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        /// <example>
        /// <code>
        /// IContainer cr = // ...
        /// using (var lifetime = cr.BeginLifetimeScope(builder =&gt; {
        ///         builder.RegisterType&lt;Foo&gt;();
        ///         builder.RegisterType&lt;Bar&gt;().As&lt;IBar&gt;(); })
        /// {
        ///     var foo = lifetime.Resolve&lt;Foo&gt;();
        /// }
        /// </code>
        /// </example>
        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return BeginLifetimeScope(MakeAnonymousTag(), configurationAction);
        }

        /// <summary>
        /// Begin a new tagged sub-scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registrations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        /// <example>
        /// <code>
        /// IContainer cr = // ...
        /// using (var lifetime = cr.BeginLifetimeScope("unitOfWork", builder =&gt; {
        ///         builder.RegisterType&lt;Foo&gt;();
        ///         builder.RegisterType&lt;Bar&gt;().As&lt;IBar&gt;(); })
        /// {
        ///     var foo = lifetime.Resolve&lt;Foo&gt;();
        /// }
        /// </code>
        /// </example>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            if (configurationAction == null) throw new ArgumentNullException(nameof(configurationAction));

            CheckNotDisposed();
            CheckTagIsUnique(tag);

            var locals = CreateScopeRestrictedRegistry(tag, configurationAction);
            var scope = new LifetimeScope(locals, this, tag);
            scope.Disposer.AddInstanceForDisposal(locals);

            if (locals.Properties.TryGetValue(MetadataKeys.ContainerBuildOptions, out var options) &&
                !((ContainerBuildOptions)options).HasFlag(ContainerBuildOptions.IgnoreStartableComponents))
            {
                StartableManager.StartStartableComponents(scope);
            }

            RaiseBeginning(scope);

            return scope;
        }

        /// <summary>
        /// Creates and setup the registry for a child scope.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registrations visible only in the child scope.</param>
        /// <returns>Registry to use for a child scope.</returns>
        /// <remarks>It is the responsibility of the caller to make sure that the registry is properly
        /// disposed of. This is generally done by adding the registry to the <see cref="Disposer"/>
        /// property of the child scope.</remarks>
        private ScopeRestrictedRegistry CreateScopeRestrictedRegistry(object tag, Action<ContainerBuilder> configurationAction)
        {
            var builder = new ContainerBuilder(new FallbackDictionary<string, object>(ComponentRegistry.Properties));

            foreach (var source in ComponentRegistry.Sources
                .Where(src => src.IsAdapterForIndividualComponents))
                builder.RegisterSource(source);

            // Issue #272: Only the most nested parent registry with HasLocalComponents is registered as an external source
            // It provides all non-adapting registrations from itself and from it's parent registries
            var parent = Traverse.Across<ISharingLifetimeScope>(this, s => s.ParentLifetimeScope)
                .Where(s => s.ComponentRegistry.HasLocalComponents)
                .Select(s => new ExternalRegistrySource(s.ComponentRegistry))
                .FirstOrDefault();

            if (parent != null)
            {
                builder.RegisterSource(parent);
            }

            configurationAction(builder);

            var locals = new ScopeRestrictedRegistry(tag, builder.Properties);
            builder.UpdateRegistry(locals);
            return locals;
        }

        /// <inheritdoc />
        public object ResolveComponent(ResolveRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            CheckNotDisposed();

            var operation = new ResolveOperation(this);
            var handler = ResolveOperationBeginning;
            handler?.Invoke(this, new ResolveOperationBeginningEventArgs(operation));
            return operation.Execute(request);
        }

        /// <summary>
        /// Gets the parent of this node of the hierarchy, or null.
        /// </summary>
        public ISharingLifetimeScope ParentLifetimeScope { get; }

        /// <summary>
        /// Gets the root of the sharing hierarchy.
        /// </summary>
        public ISharingLifetimeScope RootLifetimeScope { get; }

        /// <summary>
        /// Try to retrieve an instance based on a GUID key. If the instance
        /// does not exist, invoke <paramref name="creator"/> to create it.
        /// </summary>
        /// <param name="id">Key to look up.</param>
        /// <param name="creator">Creation function.</param>
        /// <returns>An instance.</returns>
        public object GetOrCreateAndShare(Guid id, Func<object> creator)
        {
            if (creator == null) throw new ArgumentNullException(nameof(creator));

            if (_sharedInstances.TryGetValue(id, out var result)) return result;

            lock (_synchRoot)
            {
                if (_sharedInstances.TryGetValue(id, out result)) return result;

                result = creator();
                if (_sharedInstances.ContainsKey(id))
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, LifetimeScopeResources.SelfConstructingDependencyDetected, result.GetType().FullName));

                _sharedInstances.TryAdd(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        public IDisposer Disposer { get; } = new Disposer();

        /// <summary>
        /// Gets the tag applied to the lifetime scope.
        /// </summary>
        /// <remarks>The tag applied to this scope and the contexts generated when
        /// it resolves component dependencies.</remarks>
        public object Tag { get; }

        /// <summary>
        /// Gets the services associated with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry { get; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var handler = CurrentScopeEnding;

                try
                {
                    handler?.Invoke(this, new LifetimeScopeEndingEventArgs(this));
                }
                finally
                {
                    Disposer.Dispose();
                }

                // ReSharper disable once InconsistentlySynchronizedField
                _sharedInstances.Clear();
            }

            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(LifetimeScopeResources.ScopeIsDisposed, innerException: null);
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
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            return this.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Fired when a new scope based on the current scope is beginning.
        /// </summary>
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;

        /// <summary>
        /// Fired when this scope is ending.
        /// </summary>
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;

        /// <summary>
        /// Fired when a resolve operation is beginning in this scope.
        /// </summary>
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
    }
}
