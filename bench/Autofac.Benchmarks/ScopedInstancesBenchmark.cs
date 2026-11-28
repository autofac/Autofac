// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Begins a lifetime scope, resolves a number of instance-per-lifetime-scope components in it and disposes it, which
/// measures the cost of a scope's shared instance storage. Run it with DOTNET_PROCESSOR_COUNT set to a low value as well
/// as on the machine's own processor count, since the storage used to scale with the processor count.
/// </summary>
public class ScopedInstancesBenchmark
{
    private IContainer _container = default!;

    [Params(0, 5, 20, 40)]
    public int ScopedInstanceCount
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        for (var i = 0; i < 40; i++)
        {
            builder.Register(_ => new object()).Keyed<object>(i).InstancePerLifetimeScope();
        }

        _container = builder.Build();
    }

    [GlobalCleanup]
    public void Cleanup() => _container.Dispose();

    [Benchmark]
    public void ResolveScopedInstances()
    {
        using var scope = _container.BeginLifetimeScope();
        for (var i = 0; i < ScopedInstanceCount; i++)
        {
            GC.KeepAlive(scope.ResolveKeyed<object>(i));
        }
    }
}
