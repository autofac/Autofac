// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Interface defining a source of middleware for a service's pipeline.
    /// </summary>
    public interface IServiceMiddlewareSource
    {
        /// <summary>
        /// Called when a service is first resolved; implementations can add middleware to the service using the <paramref name="pipelineBuilder"/>.
        /// </summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="availableServices">Access to the set of available service registrations.</param>
        /// <param name="pipelineBuilder">A pipeline builder that can be used to add middleware.</param>
        void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder);
    }
}
