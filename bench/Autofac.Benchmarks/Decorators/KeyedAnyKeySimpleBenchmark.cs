// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;
using Autofac.Core;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks keyed decorators when components are registered with AnyKey.
/// </summary>
public class KeyedAnyKeySimpleBenchmark : DecoratorBenchmarkBase<ICommandHandler>
{
    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<CommandHandlerOne>()
            .Keyed<ICommandHandler>(KeyedService.AnyKey);
        builder.RegisterType<CommandHandlerTwo>()
            .Keyed<ICommandHandler>(KeyedService.AnyKey);
        builder.RegisterDecorator<ICommandHandler>(
            (c, inner) => new CommandHandlerDecoratorOne(inner),
            fromKey: "handler");

        Container = builder.Build();
    }
}
