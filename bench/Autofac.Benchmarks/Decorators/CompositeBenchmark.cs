// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks resolving composites over two implementations, with and without a decorator on the implementations.
/// </summary>
public class CompositeBenchmark
{
    private IContainer _container = default!;
    private IContainer _lambdaContainer = default!;
    private IContainer _decoratedContainer = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = CreateBuilder();
        builder.RegisterComposite<CompositeCommandHandler, ICommandHandler>();
        _container = builder.Build();

        builder = CreateBuilder();
        builder.RegisterComposite<ICommandHandler>((c, handlers) => new CompositeCommandHandler(handlers));
        _lambdaContainer = builder.Build();

        builder = CreateBuilder();
        builder.RegisterComposite<CompositeCommandHandler, ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
        _decoratedContainer = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public object Resolve() => _container.Resolve<ICommandHandler>();

    [Benchmark]
    public object ResolveLambda() => _lambdaContainer.Resolve<ICommandHandler>();

    [Benchmark]
    public object ResolveDecorated() => _decoratedContainer.Resolve<ICommandHandler>();

    [GlobalCleanup]
    public void Cleanup()
    {
        _container.Dispose();
        _lambdaContainer.Dispose();
        _decoratedContainer.Dispose();
    }

    private static ContainerBuilder CreateBuilder()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<CommandHandlerOne>().As<ICommandHandler>();
        builder.RegisterType<CommandHandlerTwo>().As<ICommandHandler>();
        return builder;
    }
}
