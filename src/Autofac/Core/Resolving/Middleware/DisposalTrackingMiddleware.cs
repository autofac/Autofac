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
