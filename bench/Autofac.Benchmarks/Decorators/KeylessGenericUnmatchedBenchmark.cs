// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks a closed generic service that an open generic decorator does not decorate.
/// </summary>
/// <remarks>
/// The decorator is only examined when the service's pipeline is first built, so each
/// operation builds a new container and resolves the service once. The baseline does the
/// same without a decorator, so the difference is what the decorator costs. The keyless and
/// keyed syntaxes are handled by different sources, so both are measured.
/// </remarks>
public class KeylessGenericUnmatchedBenchmark
{
    [Benchmark(Baseline = true)]
    public object BuildAndResolveWithoutDecorator() => BuildAndResolve(_ => { });

    [Benchmark]
    public object BuildAndResolve() => BuildAndResolve(
        builder => builder.RegisterGenericDecorator(typeof(GenericCommandHandlerDecorator<>), typeof(ICommandHandler<>)));

    [Benchmark]
    public object BuildAndResolveKeyed() => BuildAndResolve(
        builder => builder.RegisterGenericDecorator(typeof(GenericCommandHandlerDecorator<>), typeof(ICommandHandler<>), fromKey: "handler"));

    private static object BuildAndResolve(Action<ContainerBuilder> registerDecorator)
    {
        var builder = new ContainerBuilder();
        builder.Register(_ => new List<Command>()).As<IReadOnlyList<Command>>();
        registerDecorator(builder);

        using var container = builder.Build();
        return container.Resolve<IReadOnlyList<Command>>();
    }
}
