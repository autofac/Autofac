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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Provides resolve middleware for locating decorators.
    /// </summary>
    internal class DecoratorMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets a singleton instance of the middleware.
        /// </summary>
        public static DecoratorMiddleware Instance { get; } = new DecoratorMiddleware();

        private DecoratorMiddleware()
        {
        }

        /// <inheritdoc />
        public PipelinePhase Phase => PipelinePhase.Decoration;

        /// <inheritdoc />
        public void Execute(ResolveRequestContextBase context, Action<ResolveRequestContextBase> next)
        {
            // Proceed down the pipeline.
            next(context);

            // If we can get a decorated instance, then use it.
            if (TryDecorateRegistration(context, out var newInstance))
            {
                context.Instance = newInstance;
            }
        }

        /// <inheritdoc />
        public override string ToString() => nameof(DecoratorMiddleware);

        private static bool TryDecorateRegistration(ResolveRequestContextBase context, [NotNullWhen(true)] out object? instance)
        {
            var service = context.Service;

            if (service is DecoratorService
                || !(service is IServiceWithType serviceWithType)
                || context.Registration is ExternalComponentRegistration)
            {
                instance = null;
                return false;
            }

            var decoratorRegistrations = context.ComponentRegistry.DecoratorsFor(serviceWithType);
            if (decoratorRegistrations.Count == 0)
            {
                instance = null;
                return false;
            }

            instance = context.Instance!;

            var serviceType = serviceWithType.ServiceType;
            var resolveParameters = context.Parameters as Parameter[] ?? context.Parameters.ToArray();

            var instanceType = instance.GetType();
            var decoratorContext = DecoratorContext.Create(instanceType, serviceType, instance);
            var decoratorCount = decoratorRegistrations.Count;

            for (var index = 0; index < decoratorCount; index++)
            {
                var decoratorRegistration = decoratorRegistrations[index];
                var decoratorService = decoratorRegistration.Services.OfType<DecoratorService>().First();
                if (!decoratorService.Condition(decoratorContext)) continue;

                var serviceParameter = new TypedParameter(serviceType, instance);
                var contextParameter = new TypedParameter(typeof(IDecoratorContext), decoratorContext);

                var invokeParameters = new Parameter[resolveParameters.Length + 2];
                for (var i = 0; i < resolveParameters.Length; i++)
                    invokeParameters[i] = resolveParameters[i];

                invokeParameters[invokeParameters.Length - 2] = serviceParameter;
                invokeParameters[invokeParameters.Length - 1] = contextParameter;

                var resolveRequest = new ResolveRequest(decoratorService, decoratorRegistration, invokeParameters, context.Registration);
                instance = context.ResolveComponentWithNewOperation(resolveRequest);

                if (index < decoratorCount - 1)
                    decoratorContext = decoratorContext.UpdateContext(instance);
            }

            return true;
        }
    }
}
