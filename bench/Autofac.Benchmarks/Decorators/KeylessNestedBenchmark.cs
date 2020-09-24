using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks a more complex case of chaining decorators using the new keyless syntax.
    /// </summary>
    public class KeylessNestedBenchmark : DecoratorBenchmarkBase<ICommandHandler>
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
            builder.RegisterDecorator<CommandHandlerDecoratorTwo, ICommandHandler>();

            Container = builder.Build();
        }
    }
}
