// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Middleware added by default to the end of all service pipelines that invokes the registration's pipeline.
    /// </summary>
    internal class RegistrationPipelineInvokeMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the singleton instance of this middleware.
        /// </summary>
        public static RegistrationPipelineInvokeMiddleware Instance { get; } = new RegistrationPipelineInvokeMiddleware();

        private RegistrationPipelineInvokeMiddleware()
        {
        }

        /// <inheritdoc/>
        public PipelinePhase Phase => PipelinePhase.ServicePipelineEnd;

        /// <inheritdoc/>
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            context.Registration.ResolvePipeline.Invoke(context);
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(RegistrationPipelineInvokeMiddleware);
    }
}
