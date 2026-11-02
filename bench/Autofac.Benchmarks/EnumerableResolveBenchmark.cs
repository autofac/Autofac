// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

    internal interface IService
    {
    }

    internal interface IManyService
    {
    }

    internal class A : IService, IManyService
    {
    }

    internal class B : IService
    {
    }

    internal class C : IService
    {
    }

    internal class D : IService
    {
    }

    internal class E : IService
    {
    }

    internal class F : IService
    {
    }
}
