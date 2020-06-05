using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Features.Decorators
{
    public class DecoratorMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly Type _decoratedServiceType;
        private readonly DecoratorService _decoratorService;
        private readonly IComponentRegistration _decoratorRegistration;

        public DecoratorMiddlewareSource(Type decoratedMiddlewareType, DecoratorService decoratorService, IComponentRegistration decoratorRegistration)
        {
            _decoratedServiceType = decoratedMiddlewareType;
            _decoratorService = decoratorService;
            _decoratorRegistration = decoratorRegistration;
        }

        public void ConfigureServicePipeline(Service service, IComponentRegistryServices availableServices, IServicePipelineBuilder pipelineConfiguration)
        {
            if (service is IServiceWithType swt && swt.ServiceType == _decoratedServiceType)
            {
                // This is the right type, decorate it.
                pipelineConfiguration.Use(new DecoratorMiddleware(swt, _decoratorService, _decoratorRegistration), MiddlewareInsertionMode.StartOfPhase);
            }
        }
    }
}
