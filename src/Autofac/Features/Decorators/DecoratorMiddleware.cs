using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Features.Decorators
{
    internal class DecoratorMiddleware : IResolveMiddleware
    {
        private readonly IServiceWithType _decoratedService;
        private readonly DecoratorService _decoratorService;
        private readonly IComponentRegistration _decoratorRegistration;

        public DecoratorMiddleware(IServiceWithType decoratedService, DecoratorService decoratorService, IComponentRegistration decoratorRegistration)
        {
            _decoratedService = decoratedService;
            _decoratorService = decoratorService;
            _decoratorRegistration = decoratorRegistration;
        }

        public PipelinePhase Phase => PipelinePhase.Decoration;

        public void Execute(ResolveRequestContextBase context, Action<ResolveRequestContextBase> next)
        {
            // Go down the pipeline first, need that instance.
            next(context);

            // Don't do this if we didn't activate an instance (for example because of decorator instance sharing).
            if (context.Instance is null)
            {
                return;
            }

            var serviceType = _decoratedService.ServiceType;

            if (context.DecoratorContext is null)
            {
                context.DecoratorContext = DecoratorContext.Create(context.Instance.GetType(), serviceType, context.Instance);
            }

            if (!_decoratorService.Condition(context.DecoratorContext))
            {
                return;
            }

            var serviceParameter = new TypedParameter(serviceType, context.DecoratorContext.CurrentInstance);
            var contextParameter = new TypedParameter(typeof(IDecoratorContext), context.DecoratorContext);

            Parameter[] resolveParameters;

            var parameterCount = context.Parameters.Count();

            if (parameterCount == 0)
            {
                resolveParameters = new Parameter[] { serviceParameter, contextParameter };
            }
            else
            {
                resolveParameters = new Parameter[parameterCount + 2];
                var idx = 0;

                foreach (var existing in context.Parameters)
                {
                    resolveParameters[idx] = existing;
                    idx++;
                }

                resolveParameters[idx++] = serviceParameter;
                resolveParameters[idx++] = contextParameter;
            }

            // We're going to define a service implementation that does not contain any actual service
            // pipeline additions.
            // Adding a service pipeline middleware to a decorator service is not valid.
            var resolveRequest = new ResolveRequest(
                _decoratorService,
                new ServiceRegistration(ServicePipelines.DefaultServicePipeline, _decoratorRegistration),
                resolveParameters,
                context.Registration);

            var decoratedInstance = context.ResolveComponentWithNewOperation(resolveRequest);

            context.Instance = decoratedInstance;

            context.DecoratorContext = context.DecoratorContext.UpdateContext(decoratedInstance);
        }
    }
}
