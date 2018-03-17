using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    internal static class DecoratorExtensions
    {
        public static object DecorateService(
            this IServiceWithType typedService,
            object instance,
            IComponentContext context,
            IEnumerable<Parameter> parameters)
        {
            var registry = context.ComponentRegistry;
            var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();
            var serviceType = typedService.ServiceType;

            var decoratorService = new DecoratorService(serviceType);

            var decorators = registry.RegistrationsFor(decoratorService)
                .OrderBy(r => r.GetRegistrationOrder())
                .Select(r => new
                {
                    Registration = r,
                    Service = r.Services.OfType<DecoratorService>().FirstOrDefault()
                })
                .ToArray();

            if (decorators.Length == 0) return instance;

            var decoratorContext = DecoratorContext.Create(instance.GetType(), serviceType, instance);

            foreach (var decorator in decorators)
            {
                if (!decorator.Service.Condition(decoratorContext)) continue;

                var serviceParameter = new TypedParameter(serviceType, instance);
                var contextParameter = new TypedParameter(typeof(IDecoratorContext), decoratorContext);
                var invokeParameters = resolveParameters.Concat(new Parameter[] { serviceParameter, contextParameter });
                instance = context.ResolveComponent(decorator.Registration, invokeParameters);

                decoratorContext = decoratorContext.UpdateContext(instance);
            }

            return instance;
        }
    }
}
