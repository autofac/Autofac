// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// Service middleware that decorates activated instances of a service.
    /// </summary>
    internal class DecoratorMiddleware : IResolveMiddleware
    {
        private readonly DecoratorService _decoratorService;
        private readonly IComponentRegistration _decoratorRegistration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratorMiddleware"/> class.
        /// </summary>
        /// <param name="decoratorService">The decorator service.</param>
        /// <param name="decoratorRegistration">The decorator registration.</param>
        public DecoratorMiddleware(DecoratorService decoratorService, IComponentRegistration decoratorRegistration)
        {
            _decoratorService = decoratorService ?? throw new ArgumentNullException(nameof(decoratorService));
            _decoratorRegistration = decoratorRegistration ?? throw new ArgumentNullException(nameof(decoratorRegistration));
        }

        /// <inheritdoc/>
        public PipelinePhase Phase => PipelinePhase.Decoration;

        /// <inheritdoc/>
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            // Go down the pipeline first, need that instance.
            next(context);

            // Don't do this if we didn't activate an instance (for example because of decorator instance sharing).
            if (context.Instance is null)
            {
                return;
            }

            // Check if decoration is disabled.
            if (context.Registration.Options.HasOption(RegistrationOptions.DisableDecoration))
            {
                return;
            }

            if (!(context.Operation is IDependencyTrackingResolveOperation dependencyTrackingResolveOperation))
            {
                // Skipping decorator middleware, since IResolveOperation is not IDependencyTrackingResolveOperation
                // Which contains required functionality EnterNewDependencyDetectionBlock
                return;
            }

            // This middleware is only ever added to IServiceWithType pipelines, so this cast will always succeed.
            var swt = (IServiceWithType)context.Service;

            var serviceType = swt.ServiceType;

            if (context.DecoratorContext is null)
            {
                context.DecoratorContext = DecoratorContext.Create(context.Instance.GetType(), serviceType, context.Instance);
            }
            else
            {
                // Update the context with the previous decorator's output.
                // Doing this here, rather than in the decorator middleware that resulted in the instance
                // means we do not need to needlessly update the decorator context on the final decorator.
                context.DecoratorContext = context.DecoratorContext.UpdateContext(context.Instance);
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

            // We're going to define a service registration that does not contain any service
            // pipeline additions.
            // Adding a service pipeline middleware to a decorator service is not valid anyway.
            var resolveRequest = new ResolveRequest(
                _decoratorService,
                new ServiceRegistration(ServicePipelines.DefaultServicePipeline, _decoratorRegistration),
                resolveParameters,
                context.Registration);

            using (dependencyTrackingResolveOperation.EnterNewDependencyDetectionBlock())
            {
                var decoratedInstance = context.ResolveComponent(resolveRequest);

                context.Instance = decoratedInstance;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(DecoratorMiddleware) + " [" + _decoratorRegistration.Activator.LimitType.Name + "]";
    }
}
