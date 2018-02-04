using System;
using Autofac.Builder;

namespace Autofac.Features.Decorators
{
    public class DecoratorActivatorData : ReflectionActivatorData
    {
        public Type ServiceType { get; }

        public DecoratorActivatorData(Type implementer, Type serviceType)
            : base(implementer)
        {
            ServiceType = serviceType;
        }
    }
}
