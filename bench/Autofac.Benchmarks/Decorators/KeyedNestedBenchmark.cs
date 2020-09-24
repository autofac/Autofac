using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks a more complex case of chaining decorators using the keyed syntax.
    /// </summary>
    public class KeyedNestedBenchmark : DecoratorBenchmarkBase<ICommandHandler>
    {
        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CommandHandlerOne>()
                .Named<ICommandHandler>("handler");
            builder.RegisterType<CommandHandlerTwo>()
                .Named<ICommandHandler>("handler");
            builder.RegisterDecorator<ICommandHandler>(
                (c, inner) => new CommandHandlerDecoratorOne(inner),
                fromKey: "handler", toKey: "decorated");
            builder.RegisterDecorator<ICommandHandler>(
                (c, inner) => new CommandHandlerDecoratorTwo(inner),
                fromKey: "decorated");

            Container = builder.Build();
        }
    }
}
