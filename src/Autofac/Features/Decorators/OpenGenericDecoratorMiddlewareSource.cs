using System;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.OpenGenerics;

namespace Autofac.Features.Decorators
{
    internal class OpenGenericDecoratorMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly DecoratorService _decoratorService;
        private readonly RegistrationData _registrationData;
        private readonly ReflectionActivatorData _activatorData;

        public OpenGenericDecoratorMiddlewareSource(DecoratorService decoratorService, RegistrationData registrationData, ReflectionActivatorData activatorData)
        {
            OpenGenericServiceBinder.EnforceBindable(activatorData.ImplementationType, registrationData.Services);

            _decoratorService = decoratorService;
            _registrationData = registrationData;
            _activatorData = activatorData;
        }

        public void ConfigureServicePipeline(Service service, IComponentRegistryServices availableServices, IServicePipelineBuilder pipelineConfiguration)
        {
            if (service is IServiceWithType swt)
            {
                var closedDecoratorService = new DecoratorService(swt.ServiceType, _decoratorService.Condition);

                if (OpenGenericServiceBinder.TryBindOpenGenericTypedService(closedDecoratorService, _registrationData.Services, _activatorData.ImplementationType, out var constructedImplementationType, out var services))
                {
                    var registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, _activatorData.ConfiguredParameters, _activatorData.ConfiguredProperties),
                        _registrationData.Lifetime,
                        _registrationData.Sharing,
                        _registrationData.Ownership,
                        services,
                        _registrationData.Metadata);

                    registration.BuildResolvePipeline(availableServices);

                    pipelineConfiguration.Use(new DecoratorMiddleware(swt, closedDecoratorService, registration), MiddlewareInsertionMode.StartOfPhase);
                }
            }
        }
    }
}
