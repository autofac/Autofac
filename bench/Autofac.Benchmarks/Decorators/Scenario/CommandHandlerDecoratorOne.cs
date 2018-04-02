using System;

namespace Autofac.Benchmarks.Decorators.Scenario
{
    public class CommandHandlerDecoratorOne : ICommandHandler
    {
        private readonly ICommandHandler _decorated;
        public CommandHandlerDecoratorOne(ICommandHandler decorated)
        {
            this._decorated = decorated;
        }
    }
}
