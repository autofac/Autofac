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
using Autofac.Resolving;
using Autofac.SelfRegistration;
using Autofac.Util;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Lifetime scope implementation.
    /// </summary>
    public class LifetimeScope : Disposable, ISharingLifetimeScope, IServiceProvider
    {
        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        readonly object _synchRoot = new object();

        readonly IComponentRegistry _componentRegistry;
        readonly ISharingLifetimeScope _root; // Root optimises singleton lookup without traversal
        readonly ISharingLifetimeScope _parent;
        readonly IDisposer _disposer = new Disposer();
        readonly IDictionary<Guid, object> _sharedInstances = new Dictionary<Guid, object>();
        object _tag;

        /// <summary>
        /// Create a lifetime scope for the provided components and nested beneath a parent.
        /// </summary>
        /// <param name="componentRegistry">Components used in the scope.</param>
        /// <param name="parent">Parent scope.</param>
        protected LifetimeScope(IComponentRegistry componentRegistry, LifetimeScope parent)
            : this(componentRegistry)
        {
            _parent = Enforce.ArgumentNotNull(parent, "parent");
            _root = _parent.RootLifetimeScope;
        }

        /// <summary>
        /// Create a lifetime scope for the provided components.
        /// </summary>
        /// <param name="componentRegistry">Components used in the scope.</param>
        public LifetimeScope(IComponentRegistry componentRegistry)
        {
            _componentRegistry = Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            _root = this;
        }

        /// <summary>
        /// Begin a new sub-scope. Instances created via the sub-scope
        /// will be disposed along with it.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            lock (_synchRoot)
            {
                var result = new LifetimeScope(_componentRegistry, this);
                result.Resolve<IndirectReference<ILifetimeScope>>().Value = result;
                return result;
            }
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
            lock (_synchRoot)
            {
                var operation = new ResolveOperation(this);
                return operation.Resolve(registration, parameters);
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
        /// Try to retrieve an instance based on a GUID key.
        /// </summary>
        /// <param name="id">Key to look up.</param>
        /// <param name="result">Instance corresponding to key.</param>
        /// <returns>True if an instance exists.</returns>
        public bool TryGetSharedInstance(Guid id, out object result)
        {
            lock (_synchRoot)
            {
                return _sharedInstances.TryGetValue(id, out result);
            }
        }

        /// <summary>
        /// Add an instance associdated with a GUID key.
        /// </summary>
        /// <param name="id">Key to associate with the instance.</param>
        /// <param name="newInstance">The instance.</param>
        public void AddSharedInstance(Guid id, object newInstance)
        {
            Enforce.ArgumentNotNull(newInstance, "newInstance");

            lock (_synchRoot)
            {
                _sharedInstances.Add(id, newInstance);
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
            set
            { 
                lock(_synchRoot)
                    _tag = value;
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
                _disposer.Dispose();

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
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return this.ResolveOptional(serviceType);
        }
    }
}
