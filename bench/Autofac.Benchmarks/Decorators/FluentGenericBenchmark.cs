using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks the simple/common use case for open generic decorators using the new fluent syntax.
    /// </summary>
    public class FluentGenericBenchmark : DecoratorBenchmarkBase<ICommandHandler<Command>>
    {
        [Setup]
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

            this.Container = builder.Build();
        }
    }
}
