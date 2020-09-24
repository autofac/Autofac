// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.OpenGenerics;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// Service middleware source that enables open generic decorators.
    /// </summary>
    internal class OpenGenericDecoratorMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly DecoratorService _decoratorService;
        private readonly RegistrationData _registrationData;
        private readonly ReflectionActivatorData _activatorData;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGenericDecoratorMiddlewareSource"/> class.
        /// </summary>
        /// <param name="decoratorService">The decorator service.</param>
        /// <param name="registrationData">The registration data for the decorator.</param>
        /// <param name="activatorData">The activator data for the decorator.</param>
        public OpenGenericDecoratorMiddlewareSource(DecoratorService decoratorService, RegistrationData registrationData, ReflectionActivatorData activatorData)
        {
            _decoratorService = decoratorService ?? throw new ArgumentNullException(nameof(decoratorService));
            _registrationData = registrationData ?? throw new ArgumentNullException(nameof(registrationData));
            _activatorData = activatorData ?? throw new ArgumentNullException(nameof(activatorData));

            OpenGenericServiceBinder.EnforceBindable(activatorData.ImplementationType, registrationData.Services);
        }

        /// <inheritdoc/>
        public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
        {
            if (service is IServiceWithType swt)
            {
                var closedDecoratorService = new DecoratorService(swt.ServiceType, _decoratorService.Condition);

                // Try to bind to the service.
                if (OpenGenericServiceBinder.TryBindOpenGenericTypedService(closedDecoratorService, _registrationData.Services, _activatorData.ImplementationType, out var constructedImplementationType, out var services))
                {
                    // Create a new closed-generic registration.
                    var registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, _activatorData.ConfiguredParameters, _activatorData.ConfiguredProperties),
                        _registrationData.Lifetime,
                        _registrationData.Sharing,
                        _registrationData.Ownership,
                        services,
                        _registrationData.Metadata);

                    // Build the resolve pipeline so we can invoke it.
                    registration.BuildResolvePipeline(availableServices);

                    // Add our closed decorator middleware to the pipeline.
                    pipelineBuilder.Use(new DecoratorMiddleware(closedDecoratorService, registration), MiddlewareInsertionMode.StartOfPhase);
                }
            }
        }
    }
}
