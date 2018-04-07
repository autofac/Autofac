using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks a more complex case of chaining decorators using the new fluent syntax.
    /// </summary>
    public class FluentNestedLambdaBenchmark : DecoratorBenchmarkBase<ICommandHandler>
    {
        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CommandHandlerOne>()
                .As<ICommandHandler>();
            builder.RegisterType<CommandHandlerTwo>()
                .As<ICommandHandler>();
            builder.RegisterDecorator<ICommandHandler>(
                (c, p, i) => new CommandHandlerDecoratorOne(i));
            builder.RegisterDecorator<ICommandHandler>(
                (c, p, i) => new CommandHandlerDecoratorTwo(i));

            this.Container = builder.Build();
        }
    }
}
