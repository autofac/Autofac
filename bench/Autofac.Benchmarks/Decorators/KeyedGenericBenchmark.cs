using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks the simple/common use case for open generic decorators using the keyed syntax.
    /// </summary>
    public class KeyedGenericBenchmark : DecoratorBenchmarkBase<ICommandHandler<Command>>
    {
        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(GenericCommandHandlerOne<>))
                .Named("handler", typeof(ICommandHandler<>));
            builder.RegisterGeneric(typeof(GenericCommandHandlerTwo<>))
                .Named("handler", typeof(ICommandHandler<>));

            builder.RegisterGenericDecorator(
                    typeof(GenericCommandHandlerDecorator<>),
                    typeof(ICommandHandler<>),
                    fromKey: "handler");

            this.Container = builder.Build();
        }
    }
}
