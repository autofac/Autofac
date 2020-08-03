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
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac
{
    /// <summary>
    /// Extension methods for adding pipeline middleware.
    /// </summary>
    public static class PipelineBuilderExtensions
    {
        private const string AnonymousDescriptor = "anonymous";

        /// <summary>
        /// Use a middleware callback in a resolve pipeline.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        /// <returns>The same builder instance.</returns>
        public static IResolvePipelineBuilder Use(this IResolvePipelineBuilder builder, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            builder.Use(phase, MiddlewareInsertionMode.EndOfPhase, callback);

            return builder;
        }

        /// <summary>
        /// Use a middleware callback in a resolve pipeline.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="insertionMode">The insertion mode specifying whether to add at the start or end of the phase.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        /// <returns>The same builder instance.</returns>
        public static IResolvePipelineBuilder Use(this IResolvePipelineBuilder builder, PipelinePhase phase, MiddlewareInsertionMode insertionMode, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            builder.Use(AnonymousDescriptor, phase, insertionMode, callback);

            return builder;
        }

        /// <summary>
        /// Use a middleware callback in a resolve pipeline.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        /// <returns>The same builder instance.</returns>
        public static IResolvePipelineBuilder Use(this IResolvePipelineBuilder builder, string descriptor, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Use(new DelegateMiddleware(descriptor, phase, callback), MiddlewareInsertionMode.EndOfPhase);

            return builder;
        }

        /// <summary>
        /// Use a middleware callback in a resolve pipeline.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="descriptor">A description for the middleware; this will show up in any resolve tracing.</param>
        /// <param name="phase">The phase of the pipeline the middleware should run at.</param>
        /// <param name="insertionMode">The insertion mode specifying whether to add at the start or end of the phase.</param>
        /// <param name="callback">
        /// A callback invoked to run your middleware.
        /// This callback takes a <see cref="ResolveRequestContext"/>, containing the context for the resolve request, plus
        /// a callback to invoke to continue the pipeline.
        /// </param>
        /// <returns>The same builder instance.</returns>
        public static IResolvePipelineBuilder Use(this IResolvePipelineBuilder builder, string descriptor, PipelinePhase phase, MiddlewareInsertionMode insertionMode, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            builder.Use(new DelegateMiddleware(descriptor, phase, callback), insertionMode);

            return builder;
        }
    }
}
