using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks the simple/common use case for decorators using the keyed syntax.
    /// </summary>
    public class KeyedSimpleBenchmark : DecoratorBenchmarkBase<ICommandHandler>
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
                fromKey: "handler");

            Container = builder.Build();
        }
    }
}
