// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Covers the container shape where one open generic component exposes several services and each
/// service has its own overriding registration. This is the only shape that makes the tracker hold
/// source implementations, so it is the one that exercises that bookkeeping; the other benchmarks
/// register single-service components and never reach it.
/// </summary>
public class OpenGenericMultiServiceBenchmark
{
    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup() => _container = BuildContainer();

    /// <summary>
    /// A fresh container per iteration, so each resolve below runs service info initialization
    /// rather than reading an already-populated registry.
    /// </summary>
    [Benchmark]
    public void BuildAndResolveAllServices()
    {
        using var container = BuildContainer();
        GC.KeepAlive(container.Resolve<IFirstService<int>>());
        GC.KeepAlive(container.Resolve<ISecondService<int>>());
        GC.KeepAlive(container.Resolve<IThirdService<int>>());
        GC.KeepAlive(container.Resolve<IFourthService<int>>());
    }

    /// <summary>
    /// Steady state on an initialized registry, confirming the bookkeeping does not leak cost into
    /// repeated resolves.
    /// </summary>
    [Benchmark]
    public void ResolveAllServicesWarm()
    {
        GC.KeepAlive(_container.Resolve<IFirstService<int>>());
        GC.KeepAlive(_container.Resolve<ISecondService<int>>());
        GC.KeepAlive(_container.Resolve<IThirdService<int>>());
        GC.KeepAlive(_container.Resolve<IFourthService<int>>());
    }

    private static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterGeneric(typeof(AllServices<>))
            .As(typeof(IFirstService<>))
            .As(typeof(ISecondService<>))
            .As(typeof(IThirdService<>))
            .As(typeof(IFourthService<>));
        builder.RegisterGeneric(typeof(FirstOnly<>)).As(typeof(IFirstService<>));
        builder.RegisterGeneric(typeof(SecondOnly<>)).As(typeof(ISecondService<>));
        builder.RegisterGeneric(typeof(ThirdOnly<>)).As(typeof(IThirdService<>));
        builder.RegisterGeneric(typeof(FourthOnly<>)).As(typeof(IFourthService<>));

        return builder.Build();
    }

    private interface IFirstService<T>
    {
    }

    private interface ISecondService<T>
    {
    }

    private interface IThirdService<T>
    {
    }

    private interface IFourthService<T>
    {
    }

    private class AllServices<T> : IFirstService<T>, ISecondService<T>, IThirdService<T>, IFourthService<T>
    {
    }

    private class FirstOnly<T> : IFirstService<T>
    {
    }

    private class SecondOnly<T> : ISecondService<T>
    {
    }

    private class ThirdOnly<T> : IThirdService<T>
    {
    }

    private class FourthOnly<T> : IFourthService<T>
    {
    }
}
