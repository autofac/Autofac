using System;
using System.Collections.Generic;

namespace Autofac.Features.Decorators
{
    public interface IDecoratorContext
    {
        Type ImplementationType { get; }

        Type ServiceType { get; }

        IEnumerable<Type> AppliedDecoratorTypes { get; }

        IEnumerable<object> AppliedDecorators { get; }

        object CurrentInstance { get; }
    }
}