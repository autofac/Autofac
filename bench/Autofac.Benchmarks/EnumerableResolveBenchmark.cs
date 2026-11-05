// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

public class EnumerableResolveBenchmark
{
    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<A>().As<IService>();
        builder.RegisterType<B>().As<IService>();
        builder.RegisterType<C>().As<IService>();
        builder.RegisterType<D>().As<IService>();
        builder.RegisterType<E>().As<IService>();
        builder.RegisterType<F>().As<IService>();

        // More than 16 elements, so sorting them by registration order doesn't use insertion sort.
        for (var i = 0; i < 20; i++)
        {
            builder.RegisterType<A>().As<IManyService>();
        }

        // Three elements under each of two keys, for the specific-key and any-key collections.
        builder.RegisterType<A>().Keyed<IKeyedService>("first");
        builder.RegisterType<B>().Keyed<IKeyedService>("first");
        builder.RegisterType<C>().Keyed<IKeyedService>("first");
        builder.RegisterType<D>().Keyed<IKeyedService>("second");
        builder.RegisterType<E>().Keyed<IKeyedService>("second");
        builder.RegisterType<F>().Keyed<IKeyedService>("second");

        _container = builder.Build();
    }

    [Benchmark]
    public object ResolveIEnumerable()
    {
        return _container.Resolve<IEnumerable<IService>>();
    }

    [Benchmark]
    public object ResolveIReadOnlyList()
    {
        return _container.Resolve<IReadOnlyList<IService>>();
    }

    [Benchmark]
    public object ResolveArray()
    {
        return _container.Resolve<IService[]>();
    }

    [Benchmark]
    public object ResolveIEnumerableOfLazy()
    {
        return _container.Resolve<IEnumerable<Lazy<IService>>>();
    }

    [Benchmark]
    public object ResolveIEnumerableOfTwenty()
    {
        return _container.Resolve<IEnumerable<IManyService>>();
    }

    [Benchmark]
    public object ResolveKeyedIEnumerable()
    {
        return _container.ResolveKeyed<IEnumerable<IKeyedService>>("first");
    }

    /// <summary>
    /// An any-key collection is shared within its scope, so each operation resolves it in a new scope
    /// to measure building it.
    /// </summary>
    [Benchmark]
    public object ResolveAnyKeyIEnumerableInNewScope()
    {
        using var scope = _container.BeginLifetimeScope();
        return scope.ResolveKeyed<IEnumerable<IKeyedService>>(KeyedService.AnyKey);
    }

    internal interface IService
    {
    }

    internal interface IKeyedService
    {
    }

    internal interface IManyService
    {
    }

    internal class A : IService, IManyService, IKeyedService
    {
    }

    internal class B : IService, IKeyedService
    {
    }

    internal class C : IService, IKeyedService
    {
    }

    internal class D : IService, IKeyedService
    {
    }

    internal class E : IService, IKeyedService
    {
    }

    internal class F : IService, IKeyedService
    {
    }
}
