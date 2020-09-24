// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Provides middleware from a parent component registry.
    /// </summary>
    internal class ExternalRegistryServiceMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly IComponentRegistry _componentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalRegistryServiceMiddlewareSource"/> class.
        /// </summary>
        /// <param name="componentRegistry">The component registry to retrieve middleware from.</param>
        public ExternalRegistryServiceMiddlewareSource(IComponentRegistry componentRegistry)
        {
            _componentRegistry = componentRegistry ?? throw new System.ArgumentNullException(nameof(componentRegistry));
        }

        /// <inheritdoc/>
        public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.UseRange(_componentRegistry.ServiceMiddlewareFor(service));
        }
    }
}
