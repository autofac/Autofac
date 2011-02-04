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
using System.Linq;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Util;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Lifetime scope implementation.
    /// </summary>
    public class LifetimeScope : Disposable, ISharingLifetimeScope, IServiceProvider
    {
        /// <summary>
        /// Protects shared instances from concurrent access. Other members and the base class are threadsafe.
        /// </summary>
        readonly object _synchRoot = new object();
        readonly IDictionary<Guid, object> _sharedInstances = new Dictionary<Guid, object>();

        readonly IComponentRegistry _componentRegistry;
        readonly ISharingLifetimeScope _root; // Root optimises singleton lookup without traversal
        readonly ISharingLifetimeScope _parent;
        readonly IDisposer _disposer = new Disposer();
        readonly object _tag;

        static internal Guid SelfRegistrationId = Guid.NewGuid();

        /// <summary>
        /// The tag applied to root scopes when no other tag is specified.
        /// </summary>
        public static readonly object RootTag = "root";

        static object MakeAnonymousTag() { return new object(); }

        private LifetimeScope()
        {
            _sharedInstances[SelfRegistrationId] = this;
        }

        /// <summary>
        /// Create a lifetime scope for the provided components and nested beneath a parent.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="componentRegistry">Components used in the scope.</param>
        /// <param name="parent">Parent scope.</param>
        protected LifetimeScope(IComponentRegistry componentRegistry, LifetimeScope parent, object tag)
            : this(componentRegistry, tag)
        {
            _parent = Enforce.ArgumentNotNull(parent, "parent");
            _root = _parent.RootLifetimeScope;
        }

        /// <summary>
        /// Create a root lifetime scope for the provided components.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="componentRegistry">Components used in the scope.</param>
        public LifetimeScope(IComponentRegistry componentRegistry, object tag)
            : this()
        {
            _componentRegistry = Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            _root = this;
            _tag = Enforce.ArgumentNotNull(tag, "tag");
        }

        /// <summary>
        /// Create a root lifetime scope for the provided components.
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
            var scope = new LifetimeScope(_componentRegistry, this, tag);
            RaiseBeginning(scope);
            return scope;
        }

        void RaiseBeginning(ILifetimeScope scope)
        {
            ChildLifetimeScopeBeginning(this, new LifetimeScopeBeginningEventArgs(scope));
        }

        /// <summary>
        /// Begin a new anonymous sub-scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        /// <example>
        /// IContainer cr = // ...
        /// using (var lifetime = cr.BeginLifetimeScope(builder =&gt; {
        ///         builder.RegisterType&lt;Foo&gt;();
        ///         builder.RegisterType&lt;Bar&gt;().As&lt;IBar&gt;(); })
        /// {
        ///     var foo = lifetime.Resolve&lt;Foo&gt;();
        /// }
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
        /// that adds component registations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        /// <example>
        /// IContainer cr = // ...
        /// using (var lifetime = cr.BeginLifetimeScope("unitOfWork", builder =&gt; {
        ///         builder.RegisterType&lt;Foo&gt;();
        ///         builder.RegisterType&lt;Bar&gt;().As&lt;IBar&gt;(); })
        /// {
        ///     var foo = lifetime.Resolve&lt;Foo&gt;();
        /// }
        /// </example>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");
            CheckNotDisposed();

            var builder =  new ContainerBuilder();

            foreach (var source in ComponentRegistry.Sources
                    .Where(src => src.IsAdapterForIndividualComponents))
                builder.RegisterSource(source);

            var parents = Traverse.Across<ISharingLifetimeScope>(this, s => s.ParentLifetimeScope)
                                    .Where(s => s.ParentLifetimeScope == null || s.ComponentRegistry != s.ParentLifetimeScope.ComponentRegistry)
                                    .Select(s => new ExternalRegistrySource(s.ComponentRegistry))
                                    .Reverse();

            foreach (var external in parents)
                builder.RegisterSource(external);
                
            configurationAction(builder);

            var locals = new ScopeRestrictedRegistry(tag);
            builder.Update(locals);
            var scope = new LifetimeScope(locals, this, tag);

            RaiseBeginning(scope);

            return scope;
        }

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="parameters">Parameters for the instance.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="Autofac.Core.Registration.ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (parameters == null) throw new ArgumentNullException("parameters");
            CheckNotDisposed();

            lock (_synchRoot)
            {
                var operation = new ResolveOperation(this);
                ResolveOperationBeginning(this, new ResolveOperationBeginningEventArgs(operation));
                return operation.Execute(registration, parameters);
            }
        }

        /// <summary>
        /// The parent of this node of the hierarchy, or null.
        /// </summary>
        public ISharingLifetimeScope ParentLifetimeScope
        {
            get { return _parent; }
        }

        /// <summary>
        /// The root of the sharing hierarchy.
        /// </summary>
        public ISharingLifetimeScope RootLifetimeScope
        {
            get { return _root; }
        }

        /// <summary>
        /// Try to retrieve an instance based on a GUID key. If the instance
        /// does not exist, invoke <paramref name="creator"/> to create it.
        /// </summary>
        /// <param name="id">Key to look up.</param>
        /// <param name="creator">Creation function.</param>
        /// <returns>An instance.</returns>
        public object GetOrCreateAndShare(Guid id, Func<object> creator)
        {
            lock (_synchRoot)
            {
                object result;
                if (!_sharedInstances.TryGetValue(id, out result))
                {
                    result = creator();
                    _sharedInstances.Add(id, result);
                }
                return result;
            }
        }

        /// <summary>
        /// The disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        public IDisposer Disposer
        {
            get { return _disposer; }
        }

        /// <summary>
        /// Tag applied to the lifetime scope.
        /// </summary>
        /// <remarks>The tag applied to this scope and the contexts generated when
        /// it resolves component dependencies.</remarks>
        public object Tag
        {
            get
            {
                return _tag; 
            }
        }

        /// <summary>
        /// Associates services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry
        {
            get { return _componentRegistry; }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentScopeEnding(this, new LifetimeScopeEndingEventArgs(this));
                _disposer.Dispose();
            }

            base.Dispose(disposing);
        }

        void CheckNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(LifetimeScopeResources.ScopeIsDisposed);
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
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            return this.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Fired when a new scope based on the current scope is beginning.
        /// </summary>
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning = delegate { };

        /// <summary>
        /// Fired when this scope is ending.
        /// </summary>
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding = delegate { };
        
        /// <summary>
        /// Fired when a resolve operation is beginning in this scope.
        /// </summary>
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning = delegate { };
    }
}
