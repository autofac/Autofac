using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks the simple/common use case for decorators using the new fluent syntax.
    /// </summary>
    public class FluentSimpleLambdaBenchmark : DecoratorBenchmarkBase<ICommandHandler>
    {
        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CommandHandlerOne>()
                .As<ICommandHandler>();
            builder.RegisterType<CommandHandlerTwo>()
                .As<ICommandHandler>();
            builder.RegisterDecorator<ICommandHandler>((c, p, i) => new CommandHandlerDecoratorOne(i));

            this.Container = builder.Build();
        }
    }
}
