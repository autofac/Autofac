// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Builds a container and creates many singletons in it, which measures the root scope's shared instance storage as it
/// grows well past its initial capacity. <see cref="Build"/> builds the same container without resolving anything, so
/// the difference between the two is the cost of creating and storing the singletons.
/// </summary>
public class SingletonInstancesBenchmark
{
    [Params(1000, 2000)]
    public int SingletonCount
    {
        get; set;
    }

    [Benchmark(Baseline = true)]
    public void Build()
    {
        using var container = BuildContainer();
    }

    [Benchmark]
    public void BuildAndResolveSingletons()
    {
        using var container = BuildContainer();
        for (var i = 0; i < SingletonCount; i++)
        {
            GC.KeepAlive(container.ResolveKeyed<object>(i));
        }
    }

    private IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();
        for (var i = 0; i < SingletonCount; i++)
        {
            builder.Register(_ => new object()).Keyed<object>(i).SingleInstance();
        }

        return builder.Build();
    }
}
