// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

namespace Autofac.Benchmarks;

/// <summary>
/// Measures resolving the implicit relationship types (<c>Lazy&lt;T&gt;</c>, <c>Owned&lt;T&gt;</c>,
/// <c>Meta&lt;T&gt;</c>, <c>IIndex&lt;K, V&gt;</c> and generated factories) from the root container.
/// </summary>
public class ImplicitRelationshipBenchmark
{
    private IContainer _container = default!;

    private Func<Component> _factory = default!;

    private Func<int, ParameterizedComponent> _parameterizedFactory = default!;

    private IIndex<string, Component> _index = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Component>()
            .Keyed<Component>("key")
            .AsSelf()
            .WithMetadata("Name", "component");
        builder.RegisterType<ParameterizedComponent>();
        _container = builder.Build();

        _factory = _container.Resolve<Func<Component>>();
        _parameterizedFactory = _container.Resolve<Func<int, ParameterizedComponent>>();
        _index = _container.Resolve<IIndex<string, Component>>();
    }

    [Benchmark]
    public object ResolveLazyAndValue()
    {
        return _container.Resolve<Lazy<Component>>().Value;
    }

    [Benchmark]
    public object ResolveLazyWithMetadataAndValue()
    {
        return _container.Resolve<Lazy<Component, ComponentMetadata>>().Value;
    }

    [Benchmark]
    public object ResolveOwnedAndDispose()
    {
        var owned = _container.Resolve<Owned<Component>>();
        owned.Dispose();
        return owned;
    }

    [Benchmark]
    public object ResolveMeta()
    {
        return _container.Resolve<Meta<Component>>();
    }

    [Benchmark]
    public object ResolveStronglyTypedMeta()
    {
        return _container.Resolve<Meta<Component, ComponentMetadata>>();
    }

    /// <summary>
    /// A child scope with its own registrations adapts the root's registrations again, so this
    /// includes creating the <c>Meta&lt;T&gt;</c> registration, without a metadata view.
    /// </summary>
    [Benchmark]
    public object ResolveMetaFromChildScope()
    {
        using var scope = _container.BeginLifetimeScope(static b => b.RegisterType<ChildComponent>());
        return scope.Resolve<Meta<Component>>();
    }

    /// <summary>
    /// Includes creating the <c>Meta&lt;T, TMetadata&gt;</c> registration and getting its metadata view provider.
    /// </summary>
    [Benchmark]
    public object ResolveStronglyTypedMetaFromChildScope()
    {
        using var scope = _container.BeginLifetimeScope(static b => b.RegisterType<ChildComponent>());
        return scope.Resolve<Meta<Component, ComponentMetadata>>();
    }

    /// <summary>
    /// Includes creating the <c>Lazy&lt;T, TMetadata&gt;</c> registration and getting its metadata view provider.
    /// </summary>
    [Benchmark]
    public object ResolveLazyWithMetadataFromChildScope()
    {
        using var scope = _container.BeginLifetimeScope(static b => b.RegisterType<ChildComponent>());
        return scope.Resolve<Lazy<Component, ComponentMetadata>>();
    }

    [Benchmark]
    public object ResolveIndex()
    {
        return _container.Resolve<IIndex<string, Component>>();
    }

    [Benchmark]
    public object IndexLookup()
    {
        return _index["key"];
    }

    [Benchmark]
    public object InvokeFunc()
    {
        return _factory();
    }

    [Benchmark]
    public object InvokeFuncWithParameter()
    {
        return _parameterizedFactory(42);
    }

    public sealed class ComponentMetadata
    {
        public string? Name
        {
            get; set;
        }
    }

    public sealed class Component
    {
    }

    public sealed class ChildComponent
    {
    }

    public sealed class ParameterizedComponent
    {
        public ParameterizedComponent(int value)
        {
            Value = value;
        }

        public int Value
        {
            get;
        }
    }
}
