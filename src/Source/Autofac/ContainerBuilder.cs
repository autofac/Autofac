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
using Autofac.Core;
using Autofac.Core.Diagnostics;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.Indexed;
using Autofac.Features.OwnedInstances;
using Autofac.Util;
using Autofac.Features.Metadata;

#if !(NET35 || WINDOWS_PHONE)
using Autofac.Features.LazyDependencies;
#endif

namespace Autofac
{
	/// <summary>
	/// Used to build an <see cref="IContainer"/> from component registrations.
	/// </summary>
    /// <example>
    /// <code>
    /// var builder = new ContainerBuilder();
    /// 
    /// builder.RegisterType&lt;Logger&gt;()
    ///     .As&lt;ILogger&gt;()
    ///     .SingleInstance();
    /// 
    /// builder.Register(c => new MessageHandler(c.Resolve&lt;ILogger&gt;()));
    /// 
    /// var container = builder.Build();
    /// // resolve components from container...
    /// </code>
    /// </example>
    /// <remarks>Most <see cref="ContainerBuilder"/> functionality is accessed
    /// via extension methods in <see cref="RegistrationExtensions"/>.</remarks>
    /// <seealso cref="IContainer"/>
    /// <see cref="RegistrationExtensions"/>
	public class ContainerBuilder
	{
        private readonly IList<Action<IComponentRegistry>> _configurationCallbacks = new List<Action<IComponentRegistry>>();
		private bool _wasBuilt;

        /// <summary>
        /// Register a callback that will be invoked when the container is configured.
        /// </summary>
        /// <remarks>This is primarily for extending the builder syntax.</remarks>
        /// <param name="configurationCallback">Callback to execute.</param>
        public virtual void RegisterCallback(Action<IComponentRegistry> configurationCallback)
        {
            _configurationCallbacks.Add(Enforce.ArgumentNotNull(configurationCallback, "configurationCallback"));
        }

		/// <summary>
		/// Create a new container with the component registrations that have been made.
		/// </summary>
		/// <remarks>
        /// Build can only be called once per <see cref="ContainerBuilder"/>
        /// - this prevents ownership issues for provided instances.
        /// Build enables support for the relationship types that come with Autofac (e.g.
        /// Func, Owned, Meta, Lazy, IEnumerable.) To exclude support for these types,
        /// first create the container, then call Update() on the builder.
		/// </remarks>
		/// <returns>A new container with the configured component registrations.</returns>
		public IContainer Build()
		{
			var result = new Container();
			Build(result.ComponentRegistry, false);
            foreach (var containerAware in result.Resolve<IEnumerable<IContainerAwareComponent>>())
                containerAware.SetContainer(result);
			return result;
		}

        /// <summary>
        /// Configure an existing container with the component registrations
        /// that have been made.
        /// </summary>
        /// <remarks>
        /// Update can only be called once per <see cref="ContainerBuilder"/>
        /// - this prevents ownership issues for provided instances.
        /// </remarks>
        /// <param name="container">An existing container to make the registrations in.</param>
        public void Update(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            Update(container.ComponentRegistry);
        }

        /// <summary>
        /// Configure an existing registry with the component registrations
        /// that have been made.
        /// </summary>
        /// <remarks>
        /// Update can only be called once per <see cref="ContainerBuilder"/>
        /// - this prevents ownership issues for provided instances.
        /// </remarks>
        /// <param name="componentRegistry">An existing registry to make the registrations in.</param>
        public void Update(IComponentRegistry componentRegistry)
        {
            if (componentRegistry == null) throw new ArgumentNullException("componentRegistry");
            Build(componentRegistry, true);
        }

	    void Build(IComponentRegistry componentRegistry, bool excludeDefaultModules)
		{
	        if (componentRegistry == null) throw new ArgumentNullException("componentRegistry");

	        if (_wasBuilt)
				throw new InvalidOperationException();

			_wasBuilt = true;

            if (!excludeDefaultModules)
                RegisterDefaultAdapters(componentRegistry);

	        foreach (var callback in _configurationCallbacks)
                callback(componentRegistry);
        }

	    void RegisterDefaultAdapters(IComponentRegistry componentRegistry)
	    {
            this.RegisterGeneric(typeof(KeyedServiceIndex<,>)).As(typeof(IIndex<,>)).InstancePerLifetimeScope();
            componentRegistry.AddRegistrationSource(new CollectionRegistrationSource());
            componentRegistry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            componentRegistry.AddRegistrationSource(new OwnedInstanceRegistrationSource());
            componentRegistry.AddRegistrationSource(new MetaRegistrationSource());
#if !(NET35 || WINDOWS_PHONE)
            componentRegistry.AddRegistrationSource(new LazyRegistrationSource());
            componentRegistry.AddRegistrationSource(new LazyWithMetadataRegistrationSource());
            componentRegistry.AddRegistrationSource(new StronglyTypedMetaRegistrationSource());
#endif
        }
	}
}