using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks.Decorators
{
    /// <summary>
    /// Benchmarks a more complex case of chaining decorators using the new keyless syntax.
    /// </summary>
    public class KeylessNestedSharedInstanceLambdaBenchmark : DecoratorBenchmarkBase<ICommandHandler>
    {
        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CommandHandlerOne>()
                .As<ICommandHandler>()
                .InstancePerLifetimeScope();
            builder.RegisterType<CommandHandlerTwo>()
                .As<ICommandHandler>()
                .InstancePerLifetimeScope();
            builder.RegisterDecorator<ICommandHandler>(
                (c, p, i) => new CommandHandlerDecoratorOne(i));
            builder.RegisterDecorator<ICommandHandler>(
                (c, p, i) => new CommandHandlerDecoratorTwo(i));

            Container = builder.Build();
        }
    }
}
