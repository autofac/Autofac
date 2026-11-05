// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Covers the core resolve path: cached shared instances, simple transients, scope creation and disposal,
/// and registration lookups, so per-operation allocations show up on their own.
/// </summary>
public class CoreResolvePathBenchmark
{
    private IContainer _container = default!;
    private ILifetimeScope _scope = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Singleton>().SingleInstance();
        builder.RegisterType<Transient>();
        builder.RegisterType<Dependency1>();
        builder.RegisterType<Dependency2>();
        builder.RegisterType<Dependency3>();
        builder.RegisterType<ThreeDependencies>();
        builder.RegisterType<Scoped>().InstancePerLifetimeScope();
        builder.RegisterType<DisposableTransient>();
        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();

        _container.Resolve<Singleton>();
        _scope.Resolve<Scoped>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _scope.Dispose();
        _container.Dispose();
    }

    [Benchmark]
    public Singleton CachedSingleton() => _container.Resolve<Singleton>();

    [Benchmark]
    public Singleton CachedSingletonFromChildScope() => _scope.Resolve<Singleton>();

    [Benchmark]
    public Scoped CachedScopedInChildScope() => _scope.Resolve<Scoped>();

    [Benchmark]
    public Transient TransientNoDependencies() => _container.Resolve<Transient>();

    [Benchmark]
    public ThreeDependencies TransientThreeDependencies() => _container.Resolve<ThreeDependencies>();

    [Benchmark]
    public void BeginAndDisposeScope()
    {
        using var scope = _container.BeginLifetimeScope();
    }

    [Benchmark]
    public void BeginAndDisposeScopeWithDisposable()
    {
        using var scope = _container.BeginLifetimeScope();
        scope.Resolve<DisposableTransient>();
    }

    [Benchmark]
    public IUnregistered? ResolveOptionalMiss() => _container.ResolveOptional<IUnregistered>();

    [Benchmark]
    public bool IsRegistered() => _container.IsRegistered<Transient>();

    public interface IUnregistered
    {
    }

    public sealed class Singleton
    {
    }

    public sealed class Scoped
    {
    }

    public sealed class Transient
    {
    }

    public sealed class Dependency1
    {
    }

    public sealed class Dependency2
    {
    }

    public sealed class Dependency3
    {
    }

    public sealed class ThreeDependencies
    {
        public ThreeDependencies(Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3)
        {
            _ = dependency1;
            _ = dependency2;
            _ = dependency3;
        }
    }

    public sealed class DisposableTransient : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
