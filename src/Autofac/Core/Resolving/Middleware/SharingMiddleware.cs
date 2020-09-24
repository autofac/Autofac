// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Checks for a shared instance of the requested registration.
    /// </summary>
    internal class SharingMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the singleton instance of the middleware.
        /// </summary>
        public static SharingMiddleware Instance { get; } = new SharingMiddleware();

        /// <inheritdoc />
        public PipelinePhase Phase => PipelinePhase.Sharing;

        /// <inheritdoc />
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            var registration = context.Registration;
            var decoratorRegistration = context.DecoratorTarget;

            var sharing = decoratorRegistration?.Sharing ?? registration.Sharing;

            if (context.ActivationScope.TryGetSharedInstance(registration.Id, decoratorRegistration?.Id, out var instance))
            {
                // Assign instance; do not continue the pipeline.
                context.Instance = instance;
            }
            else
            {
                if (sharing == InstanceSharing.Shared)
                {
                    // Assign the result of CreateSharedInstance onto the context, because if concurrency causes CreateSharedInstance to return
                    // the existing instance, the rest of the pipeline shouldn't run.
                    context.Instance = context.ActivationScope.CreateSharedInstance(registration.Id, decoratorRegistration?.Id, () =>
                    {
                        next(context);

                        if (context.Instance is null)
                        {
                            throw new InvalidOperationException("Instance null after pipeline invoke.");
                        }

                        return context.Instance;
                    });
                }
                else
                {
                    next(context);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => nameof(SharingMiddleware);
    }
}
