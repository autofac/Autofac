using System;

namespace Autofac.Benchmarks.Decorators.Scenario
{
    public class GenericCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> _decorated;
        public GenericCommandHandlerDecorator(ICommandHandler<TCommand> decorated)
        {
            _decorated = decorated;
        }
    }
}
