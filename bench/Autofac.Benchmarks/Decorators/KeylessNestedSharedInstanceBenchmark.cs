﻿using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks a more complex case of chaining decorators using the new keyless syntax.
/// </summary>
public class KeylessNestedSharedInstanceBenchmark : DecoratorBenchmarkBase<ICommandHandler>
{
    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<CommandHandlerOne>()
            .As<ICommandHandler>()
            .InstancePerLifetimeScope();
        builder.RegisterType<CommandHandlerTwo>()
            .As<ICommandHandler>()
            .InstancePerLifetimeScope();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>();

        Container = builder.Build();
    }
}
