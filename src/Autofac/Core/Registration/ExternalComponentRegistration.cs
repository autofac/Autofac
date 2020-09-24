// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    ///  A wrapper component registration created only to distinguish it from other adapted registrations.
    /// </summary>
    internal class ExternalComponentRegistration : ComponentRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalComponentRegistration"/> class.
        /// </summary>
        /// <param name="service">The service to register for.</param>
        /// <param name="target">The target registration ID.</param>
        public ExternalComponentRegistration(Service service, IComponentRegistration target)
            : base(target.Id, new NoOpActivator(target.Activator.LimitType), target.Lifetime, target.Sharing, target.Ownership, new[] { service }, target.Metadata, target)
        {
        }

        /// <inheritdoc/>
        protected override IResolvePipeline BuildResolvePipeline(IComponentRegistryServices registryServices, IResolvePipelineBuilder pipelineBuilder)
        {
            // Just use the external pipeline.
            return Target.ResolvePipeline;
        }

        private class NoOpActivator : IInstanceActivator
        {
            public NoOpActivator(Type limitType)
            {
                LimitType = limitType;
            }

            public Type LimitType { get; }

            public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
            {
                // Should never be invoked.
                throw new InvalidOperationException();
            }

            public void Dispose()
            {
                // Do not do anything here.
            }
        }
    }
}
