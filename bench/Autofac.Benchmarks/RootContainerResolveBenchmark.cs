// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Tests the performance of retrieving various simple components from a root container.
/// </summary>
public class RootContainerResolveBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Shared>().SingleInstance();
        builder.RegisterType<NonSharedReflection>();
        builder.Register(c => new NonSharedDelegate());
        _container = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public void OperatorNew()
    {
        var instance = new NonSharedDelegate();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void NonSharedReflectionResolve()
    {
        var nonShared = _container.Resolve<NonSharedReflection>();
        GC.KeepAlive(nonShared);
    }

    [Benchmark]
    public void NonSharedDelegateResolve()
    {
        var nonShared = _container.Resolve<NonSharedDelegate>();
        GC.KeepAlive(nonShared);
    }

    [Benchmark]
    public void SharedResolve()
    {
        var shared = _container.Resolve<Shared>();
        GC.KeepAlive(shared);
    }

    internal class NonSharedDelegate
    {
    }

    internal class NonSharedReflection
    {
    }

    internal class Shared
    {
    }
}
