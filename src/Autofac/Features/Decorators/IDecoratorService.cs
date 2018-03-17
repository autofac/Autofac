using System;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    public interface IDecoratorService : IServiceWithType
    {
        Func<IDecoratorContext, bool> Condition { get; }
    }
}