using System;
using System.Collections.Generic;

using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.Indexed;
using Autofac.Features.LazyDependencies;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

namespace Autofac
{
    public class LifetimeScopeBuilder
    {
        private bool _wasBuilt;
        private readonly IList<DeferredCallback> _configurationCallbacks = new List<DeferredCallback>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScopeBuilder"/> class.
        /// </summary>
        public LifetimeScopeBuilder()
            : this(new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScopeBuilder"/> class.
        /// </summary>
        /// <param name="properties">The properties used during component registration.</param>
        internal LifetimeScopeBuilder(IDictionary<string, object> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Gets the set of properties used during component registration.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
        /// context across registrations.
        /// </value>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Register a callback that will be invoked when the lifetime scope is configured.
        /// </summary>
        /// <remarks>This is primarily for extending the builder syntax.</remarks>
        /// <param name="configurationCallback">Callback to execute.</param>
        public virtual DeferredCallback RegisterCallback(Action<IComponentRegistry> configurationCallback)
        {
            if (configurationCallback == null) throw new ArgumentNullException(nameof(configurationCallback));

            var c = new DeferredCallback(configurationCallback);
            _configurationCallbacks.Add(c);
            return c;
        }

        /// <summary>
        /// Configure an existing registry with the component registrations
        /// that have been made. Primarily useful in dynamically adding registrations
        /// to a child lifetime scope.
        /// </summary>
        /// <remarks>
        /// Update can only be called once per <see cref="LifetimeScopeBuilder"/>
        /// - this prevents ownership issues for provided instances.
        /// </remarks>
        /// <param name="componentRegistry">An existing registry to make the registrations in.</param>
        internal void UpdateRegistry(IComponentRegistry componentRegistry)
        {
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            Build(componentRegistry, true);
        }

        protected void Build(IComponentRegistry componentRegistry, bool excludeDefaultModules)
        {
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));

            if (_wasBuilt)
                throw new InvalidOperationException(ContainerBuilderResources.BuildCanOnlyBeCalledOnce);

            _wasBuilt = true;

            if (!excludeDefaultModules)
                RegisterDefaultAdapters(componentRegistry);

            foreach (var callback in _configurationCallbacks)
                callback.Callback(componentRegistry);
        }

        private void RegisterDefaultAdapters(IComponentRegistry componentRegistry)
        {
            this.RegisterGeneric(typeof(KeyedServiceIndex<,>)).As(typeof(IIndex<,>)).InstancePerLifetimeScope();
            componentRegistry.AddRegistrationSource(new CollectionRegistrationSource());
            componentRegistry.AddRegistrationSource(new OwnedInstanceRegistrationSource());
            componentRegistry.AddRegistrationSource(new MetaRegistrationSource());
            componentRegistry.AddRegistrationSource(new LazyRegistrationSource());
            componentRegistry.AddRegistrationSource(new LazyWithMetadataRegistrationSource());
            componentRegistry.AddRegistrationSource(new StronglyTypedMetaRegistrationSource());
            componentRegistry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
        }
    }
}
