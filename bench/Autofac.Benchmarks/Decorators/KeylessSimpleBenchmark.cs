// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks the simple/common use case for decorators using the new keyless syntax.
/// </summary>
public class KeylessSimpleBenchmark : DecoratorBenchmarkBase<ICommandHandler>
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
        Container = builder.Build();
    }
}
