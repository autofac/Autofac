// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.Variance;

namespace Autofac.Benchmarks;

/// <summary>
/// Benchmarks binding a closed generic service to an open generic registration. A registry binds each
/// closed service once and caches the result, so these register the open generics in a new child scope
/// per operation, which binds them again. <see cref="RegisterOnly"/> shows what the scope itself costs.
/// </summary>
public class OpenGenericBindingBenchmark
{
    // Equal to the registered key but not the same object, as a key built at runtime would be.
    private static readonly string _equalKey = new('k', 3);

    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup() => _container = new ContainerBuilder().Build();

    [GlobalCleanup]
    public void Cleanup() => _container.Dispose();

    [Benchmark(Baseline = true)]
    public void RegisterOnly()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>)));
    }

    [Benchmark]
    public void Interface()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>)));
        GC.KeepAlive(scope.Resolve<IService<string>>());
    }

    [Benchmark]
    public void Self()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Service<>)));
        GC.KeepAlive(scope.Resolve<Service<string>>());
    }

    [Benchmark]
    public void BaseClass()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Derived<,>)).As(typeof(Base<,>)));
        GC.KeepAlive(scope.Resolve<Base<string, int>>());
    }

    [Benchmark]
    public void NestedGenericArgument()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(NestedService<>)).As(typeof(IService<>)));
        GC.KeepAlive(scope.Resolve<IService<IEnumerable<string>>>());
    }

    [Benchmark]
    public void Constraints()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(ConstrainedService<,>)).As(typeof(IService<,>)));
        GC.KeepAlive(scope.Resolve<IService<Model, List<Model>>>());
    }

    /// <summary>
    /// The interface declared first also closes the service, with other arguments, so binding has to look
    /// past it for the interface that maps to the service arguments.
    /// </summary>
    [Benchmark]
    public void ReversedArguments()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Reversed<,>)).As(typeof(IService<,>)));
        GC.KeepAlive(scope.Resolve<IService<string, List<int>>>());
    }

    /// <summary>
    /// The constraint is met through a generic base class two levels up.
    /// </summary>
    [Benchmark]
    public void ConstraintOnGenericBaseClass()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(EntityHandler<,>)).As(typeof(IService<,>)));
        GC.KeepAlive(scope.Resolve<IService<DerivedEntity, Model>>());
    }

    /// <summary>
    /// The constraint isn't met, so the check walks the whole base chain and interface list of the argument
    /// before the source declines.
    /// </summary>
    [Benchmark]
    public void ConstraintRejected()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(ValueSourceHandler<,>)).As(typeof(IService<,>)));
        GC.KeepAlive(scope.ResolveOptional<IService<ReferenceOnlyEntity, int>>());
    }

    /// <summary>
    /// The contravariant source checks the service's constraints for each type the argument can be
    /// substituted with: its base types, of which <see cref="object"/> is rejected, and its interfaces.
    /// </summary>
    [Benchmark]
    public void ContravariantConstraint()
    {
        using var scope = _container.BeginLifetimeScope(b =>
        {
            b.RegisterSource(new ContravariantRegistrationSource());
            b.RegisterType<ModelEntityConsumer>().As<IConsumer<ModelEntity>>();
        });
        GC.KeepAlive(scope.Resolve<IConsumer<DerivedEntity>>());
    }

    [Benchmark]
    public void MultipleServices()
    {
        using var scope = _container.BeginLifetimeScope(b => b
            .RegisterGeneric(typeof(MultiService<>))
            .As(typeof(IService<>))
            .As(typeof(IOtherService<>))
            .As(typeof(IThirdService<>)));
        GC.KeepAlive(scope.Resolve<IService<string>>());
    }

    [Benchmark]
    public void Keyed()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Service<>)).Keyed("key", typeof(IService<>)));
        GC.KeepAlive(scope.ResolveKeyed<IService<string>>("key"));
    }

    [Benchmark]
    public void KeyedWithEqualKey()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(Service<>)).Keyed("kkk", typeof(IService<>)));
        GC.KeepAlive(scope.ResolveKeyed<IService<string>>(_equalKey));
    }

    [Benchmark]
    public void Decorator()
    {
        using var scope = _container.BeginLifetimeScope(b =>
        {
            b.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>));
            b.RegisterGenericDecorator(typeof(ServiceDecorator<>), typeof(IService<>));
        });
        GC.KeepAlive(scope.Resolve<IService<string>>());
    }

    [Benchmark]
    public void Delegate()
    {
        using var scope = _container.BeginLifetimeScope(b => b.RegisterGeneric((context, types) => new Service<string>()).As(typeof(IService<>)));
        GC.KeepAlive(scope.Resolve<IService<string>>());
    }

    /// <summary>
    /// Every open generic source in the registry is asked about every closed service, so a resolve
    /// against many open generic registrations is mostly sources that don't match.
    /// </summary>
    [Benchmark]
    public void AmongNonMatchingSources()
    {
        using var scope = _container.BeginLifetimeScope(b =>
        {
            b.RegisterGeneric(typeof(OtherService<>)).As(typeof(IOtherService<>));
            b.RegisterGeneric(typeof(ThirdService<>)).As(typeof(IThirdService<>));
            b.RegisterGeneric(typeof(OtherService<>)).Keyed("key", typeof(IOtherService<>));
            b.RegisterGeneric(typeof(ThirdService<>)).Keyed("key", typeof(IThirdService<>));
            b.RegisterGeneric((context, types) => new OtherService<string>()).As(typeof(IOtherService<>));
            b.RegisterGeneric((context, types) => new ThirdService<string>()).As(typeof(IThirdService<>));
            b.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>));
        });
        GC.KeepAlive(scope.Resolve<IService<string>>());
    }

    private interface IService<T>
    {
    }

    private interface IService<T1, T2>
    {
    }

    private interface IOtherService<T>
    {
    }

    private interface IThirdService<T>
    {
    }

    private interface IConsumer<in TEntity>
        where TEntity : Entity<Model>
    {
    }

    private interface IReference<T>
    {
    }

    private interface IValueSource<T>
    {
    }

    private class Service<T> : IService<T>
    {
    }

    private class ServiceDecorator<T> : IService<T>
    {
        public ServiceDecorator(IService<T> inner)
        {
            Inner = inner;
        }

        public IService<T> Inner
        {
            get;
        }
    }

    private class OtherService<T> : IOtherService<T>
    {
    }

    private class ThirdService<T> : IThirdService<T>
    {
    }

    private class MultiService<T> : IService<T>, IOtherService<T>, IThirdService<T>
    {
    }

    private class NestedService<T> : IService<IEnumerable<T>>
    {
    }

    private class ConstrainedService<TModel, TList> : IService<TModel, TList>
        where TModel : class, new()
        where TList : IList<TModel>
    {
    }

    private class Base<T1, T2>
    {
    }

    private class Derived<T1, T2> : Base<T2, T1>
    {
    }

    private class Model
    {
    }

    private class Reversed<T1, T2> : IService<T2, List<T1>>, IService<T1, T2>
    {
    }

    private class Entity<TModel>
    {
    }

    private class ModelEntity : Entity<Model>
    {
    }

    private class DerivedEntity : ModelEntity
    {
    }

    private class ReferenceOnlyEntity : DerivedEntity, IReference<string>
    {
    }

    private class EntityHandler<TEntity, TModel> : IService<TEntity, TModel>
        where TEntity : Entity<TModel>
    {
    }

    private class ValueSourceHandler<TEntity, TValue> : IService<TEntity, TValue>
        where TEntity : IValueSource<TValue>
        where TValue : struct
    {
    }

    private class ModelEntityConsumer : IConsumer<ModelEntity>
    {
    }
}
