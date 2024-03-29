﻿using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks the shared instance case for decorators using the new keyless syntax.
/// </summary>
public class KeylessSimpleSharedInstanceBenchmark : DecoratorBenchmarkBase<ICommandHandler>
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
        Container = builder.Build();
    }
}
