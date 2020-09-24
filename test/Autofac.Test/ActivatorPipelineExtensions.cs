// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Test
{
    /// <summary>
    /// Extension methods to help test activator pipelines.
    /// </summary>
    public static class ActivatorPipelineExtensions
    {
        /// <summary>
        /// Get an invoker for an activator's pipeline.
        /// </summary>
        /// <param name="activator">The activator.</param>
        /// <param name="registry">The applicable component registry.</param>
        /// <returns>A func to call that invokes the pipeline.</returns>
        public static Func<ILifetimeScope, IEnumerable<Parameter>, object> GetPipelineInvoker(this IInstanceActivator activator, IComponentRegistry registry)
        {
            return GetPipelineInvoker<object>(activator, registry);
        }

        /// <summary>
        /// Get an invoker for an activator's pipeline.
        /// </summary>
        /// <param name="activator">The activator.</param>
        /// <param name="registry">The applicable component registry.</param>
        /// <returns>A func to call that invokes the pipeline.</returns>
        public static Func<ILifetimeScope, IEnumerable<Parameter>, T> GetPipelineInvoker<T>(this IInstanceActivator activator, IComponentRegistry registry)
        {
            var services = new RegistryServices(registry);
            var pipelineBuilder = new ResolvePipelineBuilder(PipelineType.Registration);

            activator.ConfigurePipeline(services, pipelineBuilder);

            var built = pipelineBuilder.Build();

            return (scope, parameters) =>
            {
                // To get the sharing scope from what might be a container, we're going to resolve the lifetime scope.
                var lifetimeScope = scope.Resolve<ILifetimeScope>() as LifetimeScope;

                var request = new DefaultResolveRequestContext(
                    new ResolveOperation(lifetimeScope, lifetimeScope.DiagnosticSource),
                    new ResolveRequest(new TypedService(typeof(T)), Mocks.GetResolvableImplementation(), parameters),
                    lifetimeScope,
                    lifetimeScope.DiagnosticSource);

                built.Invoke(request);

                return (T)request.Instance;
            };
        }

        private class RegistryServices : IComponentRegistryServices
        {
            private readonly IComponentRegistry _registry;

            public RegistryServices(IComponentRegistry registry)
            {
                _registry = registry;
            }

            public bool IsRegistered(Service service)
            {
                return _registry.IsRegistered(service);
            }

            public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
            {
                return _registry.RegistrationsFor(service);
            }

            public bool TryGetRegistration(Service service, [NotNullWhen(true)] out IComponentRegistration registration)
            {
                return _registry.TryGetRegistration(service, out registration);
            }
        }
    }
}
