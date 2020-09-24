using System;

namespace Autofac.Benchmarks.Decorators.Scenario
{
    public class CommandHandlerDecoratorTwo : ICommandHandler
    {
        public CommandHandlerDecoratorTwo(ICommandHandler decorated)
        {
        }
    }
}
