using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks the simple/common use case for open generic decorators using the new keyless syntax.
    /// </summary>
    public class KeylessGenericBenchmark : DecoratorBenchmarkBase<ICommandHandler<Command>>
    {
        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(GenericCommandHandlerOne<>))
                .As(typeof(ICommandHandler<>));
            builder.RegisterGeneric(typeof(GenericCommandHandlerTwo<>))
                .As(typeof(ICommandHandler<>));
            builder.RegisterGenericDecorator(
                typeof(GenericCommandHandlerTwo<>),
                typeof(ICommandHandler<>));

            Container = builder.Build();
        }
    }
}
