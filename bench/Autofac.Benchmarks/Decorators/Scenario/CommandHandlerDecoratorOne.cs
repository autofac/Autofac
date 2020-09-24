using System;

namespace Autofac.Benchmarks.Decorators.Scenario
{
    public class CommandHandlerDecoratorOne : ICommandHandler
    {
        public CommandHandlerDecoratorOne(ICommandHandler decorated)
        {
        }
    }
}
