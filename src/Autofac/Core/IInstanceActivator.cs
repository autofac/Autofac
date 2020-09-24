// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// Activates component instances.
    /// </summary>
    public interface IInstanceActivator : IDisposable
    {
        /// <summary>
        /// Allows an implementation to add middleware to a registration's resolve pipeline.
        /// </summary>
        /// <param name="componentRegistryServices">Provides access to the set of all available services.</param>
        /// <param name="pipelineBuilder">The registration's pipeline builder.</param>
        void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder);

        /// <summary>
        /// Gets the most specific type that the component instances are known to be castable to.
        /// </summary>
        Type LimitType { get; }
    }
}
