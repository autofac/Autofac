// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Benchmarks resolving a keyed service satisfied by a <see cref="KeyedService.AnyKey"/>
/// registration. Each scope with its own registry builds its own adapter registration, so
/// <see cref="ResolveAnyKeyFromNewChildScope"/> against <see cref="ResolveDirectKeyFromNewChildScope"/>
/// shows what that adapter costs.
/// </summary>
public class KeyedAnyKeyChildScopeBenchmark
{
    private IContainer _container = default!;
    private ILifetimeScope _existingScope = default!;

    [Benchmark]
    public void ResolveAnyKeyFromNewChildScope()
    {
        using (var scope = _container.BeginLifetimeScope(b => b.RegisterType<Dependency>()))
        {
            var instance = scope.ResolveKeyed<IAnyKeyService>("key");
            GC.KeepAlive(instance);
        }
    }

    [Benchmark]
    public void ResolveDirectKeyFromNewChildScope()
    {
        using (var scope = _container.BeginLifetimeScope(b => b.RegisterType<Dependency>()))
        {
            var instance = scope.ResolveKeyed<IDirectKeyService>("key");
            GC.KeepAlive(instance);
        }
    }

    [Benchmark]
    public void ResolveAnyKeyFromExistingChildScope()
    {
        var instance = _existingScope.ResolveKeyed<IAnyKeyService>("key");
        GC.KeepAlive(instance);
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<AnyKeyService>().As<IAnyKeyService>().Keyed<IAnyKeyService>(KeyedService.AnyKey);
        builder.RegisterType<DirectKeyService>().As<IDirectKeyService>().Keyed<IDirectKeyService>("key");
        _container = builder.Build();
        _existingScope = _container.BeginLifetimeScope(b => b.RegisterType<Dependency>());
        _existingScope.ResolveKeyed<IAnyKeyService>("key");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _existingScope.Dispose();
        _container.Dispose();
    }

    internal interface IAnyKeyService
    {
    }

    internal interface IDirectKeyService
    {
    }

    internal class AnyKeyService : IAnyKeyService
    {
    }

    internal class DirectKeyService : IDirectKeyService
    {
    }

    internal class Dependency
    {
    }
}
