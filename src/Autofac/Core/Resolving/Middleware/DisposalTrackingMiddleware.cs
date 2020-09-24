// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Ensures activated instances are tracked.
    /// </summary>
    internal class DisposalTrackingMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="DisposalTrackingMiddleware"/>.
        /// </summary>
        public static DisposalTrackingMiddleware Instance { get; } = new DisposalTrackingMiddleware();

        private DisposalTrackingMiddleware()
        {
            // Singleton use only.
        }

        /// <inheritdoc />
        public PipelinePhase Phase => PipelinePhase.Activation;

        /// <inheritdoc />
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            next(context);

            if (context.Registration.Ownership == InstanceOwnership.OwnedByLifetimeScope)
            {
                // The fact this adds instances for disposal agnostic of the activator is
                // important. The ProvidedInstanceActivator will NOT dispose of the provided
                // instance once the instance has been activated - assuming that it will be
                // done during the lifetime scope's Disposer executing.
                if (context.Instance is IDisposable instanceAsDisposable)
                {
                    context.ActivationScope.Disposer.AddInstanceForDisposal(instanceAsDisposable);
                }
                else if (context.Instance is IAsyncDisposable asyncDisposableInstance)
                {
                    context.ActivationScope.Disposer.AddInstanceForAsyncDisposal(asyncDisposableInstance);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => nameof(DisposalTrackingMiddleware);
    }
}
