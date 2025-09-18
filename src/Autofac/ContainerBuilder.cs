// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.Indexed;
using Autofac.Features.LazyDependencies;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

namespace Autofac;

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
public sealed class ContainerBuilder
{
    private static int _builderAlreadyAllocated;

    private readonly bool _clearRegistrationCaches;
    private readonly List<DeferredCallback> _configurationCallbacks = new List<DeferredCallback>();
    private BuildCallbackService? _buildCallbacks;
    private bool _wasBuilt;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
    /// </summary>
    public ContainerBuilder()
        : this(new Dictionary<string, object?>())
    {
        // If this is not the first container builder we have constructed in this process,
        // it's entirely likely we are going to create more (for example, in unit tests).
        // So, all container builders after the first will preserve cache's that
        // only have the RegistrationCacheUsage.Registration flag, to improve
        // the performance of subsequent container builds.
        _clearRegistrationCaches = IsFirstContainerBuilder();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
    /// </summary>
    /// <param name="properties">The properties used during component registration.</param>
    internal ContainerBuilder(IDictionary<string, object?> properties)
        : this(properties, new ComponentRegistryBuilder(new DefaultRegisteredServicesTracker(), properties))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
    /// </summary>
    /// <param name="componentRegistryBuilder">The builder to use for building the underlying <see cref="IComponentRegistry" />.</param>
    internal ContainerBuilder(IComponentRegistryBuilder componentRegistryBuilder)
        : this(new Dictionary<string, object?>(), componentRegistryBuilder)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
    /// </summary>
    /// <param name="properties">The properties used during component registration.</param>
    /// <param name="componentRegistryBuilder">The builder to use for building the underlying <see cref="IComponentRegistry" />.</param>
    internal ContainerBuilder(IDictionary<string, object?> properties, IComponentRegistryBuilder componentRegistryBuilder)
    {
        Properties = properties;
        ComponentRegistryBuilder = componentRegistryBuilder;
    }

    /// <summary>
    /// Gets the builder to use for building the underlying <see cref="IComponentRegistry" />.
    /// </summary>
    public IComponentRegistryBuilder ComponentRegistryBuilder { get; }

    /// <summary>
    /// Gets the set of properties used during component registration.
    /// </summary>
    /// <value>
    /// An <see cref="IDictionary{TKey, TValue}"/> that can be used to share
    /// context across registrations.
    /// </value>
    public IDictionary<string, object?> Properties { get; }

    /// <summary>
    /// Register a callback that will be invoked when the container is configured.
    /// </summary>
    /// <remarks>This is primarily for extending the builder syntax.</remarks>
    /// <param name="configurationCallback">Callback to execute.</param>
    /// <returns>A <see cref="DeferredCallback"/> that can be used to unregister the callback.</returns>
    public DeferredCallback RegisterCallback(Action<IComponentRegistryBuilder> configurationCallback)
    {
        var c = new DeferredCallback(configurationCallback);
        _configurationCallbacks.Add(c);
        return c;
    }

    /// <summary>
    /// Register a callback that will be invoked when the container (or lifetime scope) is built.
    /// </summary>
    /// <param name="buildCallback">Callback to execute.</param>
    /// <returns>The <see cref="ContainerBuilder"/> instance to continue registration calls.</returns>
    /// <remarks>
    /// The argument to the registered build callback will be an instance of the derived <see cref="IContainer" /> if this
    /// builder is being used to build a fresh container, or an <see cref="ILifetimeScope" /> if it's being used to
    /// create custom registrations for a new lifetime scope.
    /// </remarks>
    public ContainerBuilder RegisterBuildCallback(Action<ILifetimeScope> buildCallback)
    {
        if (buildCallback == null)
        {
            throw new ArgumentNullException(nameof(buildCallback));
        }

        if (_buildCallbacks == null)
        {
            _buildCallbacks = new BuildCallbackService();

            // Register the service.
            this.RegisterInstance(_buildCallbacks);
        }

        _buildCallbacks.AddCallback(buildCallback);

        return this;
    }

    /// <summary>
    /// Create a new container with the component registrations that have been made.
    /// </summary>
    /// <param name="options">Options that influence the way the container is initialized.</param>
    /// <remarks>
    /// Build can only be called once per <see cref="ContainerBuilder"/>
    /// - this prevents ownership issues for provided instances.
    /// Build enables support for the relationship types that come with Autofac (e.g.
    /// Func, Owned, Meta, Lazy, IEnumerable.) To exclude support for these types,
    /// first create the container, then call Update() on the builder.
    /// </remarks>
    /// <returns>A new container with the configured component registrations.</returns>
    public IContainer Build(ContainerBuildOptions options = ContainerBuildOptions.None)
    {
        Properties[MetadataKeys.ContainerBuildOptions] = options;

#pragma warning disable CA2000 // Dispose objects before losing scope
        ComponentRegistryBuilder.Register(new SelfComponentRegistration());
#pragma warning restore CA2000 // Dispose objects before losing scope

        Build(ComponentRegistryBuilder, (options & ContainerBuildOptions.ExcludeDefaultModules) != ContainerBuildOptions.None);

        var componentRegistry = ComponentRegistryBuilder.Build();

        var result = new Container(componentRegistry);
        if ((options & ContainerBuildOptions.IgnoreStartableComponents) == ContainerBuildOptions.None)
        {
            StartableManager.StartStartableComponents(Properties, result);
        }

        // Run any build callbacks.
        BuildCallbackManager.RunBuildCallbacks(result);

        // Allow the reflection cache to empty any registration-time caches to save memory.
        ReflectionCacheSet.Shared.OnContainerBuildClearCaches(_clearRegistrationCaches);

        return result;
    }

    /// <summary>
    /// Configure an existing registry with the component registrations
    /// that have been made. Primarily useful in dynamically adding registrations
    /// to a child lifetime scope.
    /// </summary>
    /// <remarks>
    /// Update can only be called once per <see cref="ContainerBuilder"/>
    /// - this prevents ownership issues for provided instances.
    /// </remarks>
    /// <param name="componentRegistry">An existing registry to make the registrations in.</param>
    internal void UpdateRegistry(IComponentRegistryBuilder componentRegistry)
    {
        Build(componentRegistry, true);
    }

    private void Build(IComponentRegistryBuilder componentRegistry, bool excludeDefaultModules)
    {
        if (componentRegistry == null)
        {
            throw new ArgumentNullException(nameof(componentRegistry));
        }

        if (_wasBuilt)
        {
            throw new InvalidOperationException(ContainerBuilderResources.BuildCanOnlyBeCalledOnce);
        }

        _wasBuilt = true;

        if (!excludeDefaultModules)
        {
            RegisterDefaultAdapters(componentRegistry);
        }

        foreach (var callback in _configurationCallbacks)
        {
            callback.Callback(componentRegistry);
        }
    }

    private void RegisterDefaultAdapters(IComponentRegistryBuilder componentRegistry)
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

    private static bool IsFirstContainerBuilder()
    {
        // First container will start with a value of 0, we will try and set it to 1;
        // if the value is 0, it means it's the first builder.
        return Interlocked.CompareExchange(ref _builderAlreadyAllocated, 1, 0) == 0;
    }
}
