// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks the shared instance case for decorators using the new keyless syntax.
/// </summary>
public class KeylessSimpleSharedInstanceLambdaBenchmark : DecoratorBenchmarkBase<ICommandHandler>
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
        builder.RegisterDecorator<ICommandHandler>((c, p, i) => new CommandHandlerDecoratorOne(i));
        Container = builder.Build();
    }
}
