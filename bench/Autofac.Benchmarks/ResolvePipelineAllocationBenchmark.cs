// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Tests the per-resolve allocation cost of running the resolve pipeline. Each
/// resolve walks the built middleware chain, so a closure allocated per stage
/// per invocation (rather than once at pipeline build time) shows up here as
/// extra allocations that scale with graph depth. See issue #1493.
/// </summary>
public class ResolvePipelineAllocationBenchmark
{
    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().As<IService>();
        builder.RegisterType<Dependency>().As<IDependency>();
        builder.RegisterType<Consumer>().As<IConsumer>();
        _container = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public IService NoDependencies() => _container.Resolve<IService>();

    [Benchmark]
    public IConsumer OneDependency() => _container.Resolve<IConsumer>();

    public interface IService
    {
    }

    public interface IDependency
    {
    }

    public interface IConsumer
    {
    }

    public sealed class Service : IService
    {
    }

    public sealed class Dependency : IDependency
    {
    }

    public sealed class Consumer : IConsumer
    {
        public Consumer(IDependency dependency)
        {
            _ = dependency;
        }
    }
}
