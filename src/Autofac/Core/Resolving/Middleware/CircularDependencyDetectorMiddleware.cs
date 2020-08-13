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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Common stage. Added to the start of all component pipelines.
    /// </summary>
    internal class CircularDependencyDetectorMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Defines the default max resolve depth.
        /// </summary>
        public const int DefaultMaxResolveDepth = 50;

        /// <summary>
        /// Gets the default instance of <see cref="CircularDependencyDetectorMiddleware"/>.
        /// </summary>
        public static CircularDependencyDetectorMiddleware Default { get; } = new CircularDependencyDetectorMiddleware(DefaultMaxResolveDepth);

        private readonly int _maxResolveDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularDependencyDetectorMiddleware"/> class.
        /// </summary>
        /// <param name="maxResolveDepth">The max resolve depth.</param>
        public CircularDependencyDetectorMiddleware(int maxResolveDepth)
        {
            _maxResolveDepth = maxResolveDepth;
        }

        /// <inheritdoc/>
        public PipelinePhase Phase => PipelinePhase.ResolveRequestStart;

        /// <inheritdoc/>
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            if (!(context.Operation is IDependencyTrackingResolveOperation dependencyTrackingResolveOperation))
            {
                // Skipping circular dependency detection, since IResolveOperation is not IDependencyTrackingResolveOperation
                // Which contains the actual RequestStack using SegmentStack
                next(context);
                return;
            }

            var activationDepth = context.Operation.RequestDepth;

            if (activationDepth > _maxResolveDepth)
            {
#if NETSTANDARD2_1
                // In .NET Standard 2.1 we will try and keep going until we run out of stack space.
                if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
                {
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorMessages.MaxDepthExceeded, context.Service));
                }
#else
                // Pre .NET Standard 2.1 we just end at 50.
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorMessages.MaxDepthExceeded, context.Service));
#endif
            }

            var requestStack = dependencyTrackingResolveOperation.RequestStack;

            // The first one is the current resolve request.
            // Do our circular dependency check.
            if (activationDepth > 1)
            {
                var registration = context.Registration;

                foreach (var requestEntry in requestStack)
                {
                    if (requestEntry.Registration == registration)
                    {
                        throw new DependencyResolutionException(string.Format(
                            CultureInfo.CurrentCulture,
                            CircularDependencyDetectorMessages.CircularDependency,
                            CreateDependencyGraphTo(registration, requestStack)));
                    }
                }
            }

            requestStack.Push(context);

            try
            {
                // Circular dependency check is done, move to the next stage.
                next(context);
            }
            finally
            {
                requestStack.Pop();
            }
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(CircularDependencyDetectorMiddleware);

        private static string CreateDependencyGraphTo(IComponentRegistration registration, IEnumerable<ResolveRequestContext> requestStack)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (requestStack == null)
            {
                throw new ArgumentNullException(nameof(requestStack));
            }

            var dependencyGraph = Display(registration);

            return requestStack.Select(a => a.Registration)
                .Aggregate(dependencyGraph, (current, requestor) => Display(requestor) + " -> " + current);
        }

        private static string Display(IComponentRegistration registration)
        {
            return registration.Activator.DisplayName();
        }
    }
}
