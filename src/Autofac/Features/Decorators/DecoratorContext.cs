using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Features.Decorators
{
    public sealed class DecoratorContext : IDecoratorContext
    {
        public Type ImplementationType { get; private set; }

        public Type ServiceType { get; private set; }

        public IEnumerable<Type> AppliedDecoratorTypes { get; private set; }

        public IEnumerable<object> AppliedDecorators { get; private set; }

        public object CurrentInstance { get; private set; }

        private DecoratorContext()
        {
        }

        public static DecoratorContext Create(Type implementationType, Type serviceType, object implementationInstance)
        {
            var context = new DecoratorContext
            {
                ImplementationType = implementationType,
                ServiceType = serviceType,
                AppliedDecorators = Enumerable.Empty<object>(),
                AppliedDecoratorTypes = Enumerable.Empty<Type>(),
                CurrentInstance = implementationInstance
            };
            return context;
        }

        public DecoratorContext UpdateContext(object decoratorInstance)
        {
            var context = new DecoratorContext
            {
                ImplementationType = ImplementationType,
                ServiceType = ServiceType,
                AppliedDecorators = AppliedDecorators.Concat(new[] { decoratorInstance }),
                AppliedDecoratorTypes = AppliedDecoratorTypes.Concat(new[] { decoratorInstance.GetType() }),
                CurrentInstance = decoratorInstance
            };
            return context;
        }
    }
}
