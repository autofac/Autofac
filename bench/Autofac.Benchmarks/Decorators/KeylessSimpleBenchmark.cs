using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
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
}
