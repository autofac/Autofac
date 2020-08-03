// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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

            context.DecoratorContext ??= DecoratorContext.Create(context.Instance.GetType(), serviceType, context.Instance);

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

                context.DecoratorContext = context.DecoratorContext.UpdateContext(decoratedInstance);
            }
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(DecoratorMiddleware) + " [" + _decoratorRegistration.Activator.LimitType.Name + "]";
    }
}
