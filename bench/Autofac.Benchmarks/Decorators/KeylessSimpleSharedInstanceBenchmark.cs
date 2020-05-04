using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Autofac.Benchmarks.Decorators
{
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
                .SingleInstance();
            builder.RegisterType<CommandHandlerTwo>()
                .As<ICommandHandler>()
                .SingleInstance();
            builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
            this.Container = builder.Build();
        }
    }
}
