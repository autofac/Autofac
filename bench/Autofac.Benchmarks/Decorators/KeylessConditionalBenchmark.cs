// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks a chain where a conditional decorator inspects the decorator context and is not applied.
/// </summary>
public class KeylessConditionalBenchmark : DecoratorBenchmarkBase<ICommandHandler>
{
    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<CommandHandlerOne>()
            .As<ICommandHandler>();
        builder.RegisterType<CommandHandlerTwo>()
            .As<ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>(context => context.AppliedDecoratorTypes.Count == 0);

        Container = builder.Build();
    }
}
