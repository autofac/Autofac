using System;

namespace Autofac.Benchmarks.Decorators.Scenario
{
    public class GenericCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        public GenericCommandHandlerDecorator(ICommandHandler<TCommand> decorated)
        {
        }
    }
}
