// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Looks services up in a component registry: in the container, where the service is already known,
/// and in a new child scope, where the first lookup queries the registration sources.
/// </summary>
public class RegistryLookupBenchmark
{
    private static readonly TypedService _registeredService = new(typeof(Component));

    private static readonly TypedService _unregisteredService = new(typeof(NeverRegistered));

    private IContainer _container = default!;

    [Benchmark]
    public bool TryGetServiceRegistration()
    {
        return _container.ComponentRegistry.TryGetServiceRegistration(_registeredService, out _);
    }

    [Benchmark]
    public bool IsRegistered()
    {
        return _container.ComponentRegistry.IsRegistered(_registeredService);
    }

    [Benchmark]
    public void BeginChildScopeWithConfiguration()
    {
        using var scope = _container.BeginLifetimeScope(b => { });
    }

    [Benchmark]
    public bool FirstLookupInChildScope()
    {
        using var scope = _container.BeginLifetimeScope(b => { });
        return scope.ComponentRegistry.TryGetServiceRegistration(_registeredService, out _);
    }

    [Benchmark]
    public bool FirstLookupOfUnregisteredServiceInChildScope()
    {
        using var scope = _container.BeginLifetimeScope(b => { });
        return scope.ComponentRegistry.TryGetServiceRegistration(_unregisteredService, out _);
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Component>();
        _container = builder.Build();

        // The container learns about the unregistered service once, as any lookup of it would.
        _container.ComponentRegistry.TryGetServiceRegistration(_unregisteredService, out _);
    }

    internal sealed class Component
    {
    }

    internal sealed class NeverRegistered
    {
    }
}
