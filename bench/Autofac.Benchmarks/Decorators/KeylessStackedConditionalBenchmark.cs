// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks a chain of stacked decorators where conditional decorators are skipped between applied ones.
/// </summary>
public class KeylessStackedConditionalBenchmark : DecoratorBenchmarkBase<ICommandHandler>
{
    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<CommandHandlerOne>()
            .As<ICommandHandler>();
        builder.RegisterType<CommandHandlerTwo>()
            .As<ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>(context => false);
        builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>(context => false);
        builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>();

        Container = builder.Build();
    }
}
