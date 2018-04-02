using System;
using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks a more complex case of chaining decorators using the new fluent syntax.
    /// </summary>
    public class FluentNestedBenchmark : DecoratorBenchmarkBase<ICommandHandler>
    {
        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CommandHandlerOne>()
                .As<ICommandHandler>();
            builder.RegisterType<CommandHandlerTwo>()
                .As<ICommandHandler>();
            builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
            builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>();
            this.Container = builder.Build();
        }
    }
}
