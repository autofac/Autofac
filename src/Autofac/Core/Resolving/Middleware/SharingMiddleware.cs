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
