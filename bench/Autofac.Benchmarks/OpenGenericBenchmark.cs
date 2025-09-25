// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

public class OpenGenericBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>));
        _container = builder.Build();
    }

    [Benchmark]
    public void ResolveOpenGeneric() => _container.Resolve<IService<string>>();

    private interface IService<T>
    {
    }

    private class Service<T> : IService<T>
    {
    }
}
