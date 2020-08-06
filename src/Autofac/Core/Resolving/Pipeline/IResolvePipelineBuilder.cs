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

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Provides the ability to build a resolve pipeline from a set of middleware.
    /// </summary>
    public interface IResolvePipelineBuilder
    {
        /// <summary>
        /// Construct a concrete resolve pipeline from this builder.
        /// </summary>
        /// <returns>A built pipeline.</returns>
        IResolvePipeline Build();

        /// <summary>
        /// Gets the set of middleware currently registered.
        /// </summary>
        IEnumerable<IResolveMiddleware> Middleware { get; }

        /// <summary>
        /// Gets the type of the pipeline this instance will build.
        /// </summary>
        PipelineType Type { get; }

        /// <summary>
        /// Use a piece of middleware in a resolve pipeline.
        /// </summary>
        /// <param name="middleware">The middleware instance.</param>
        /// <param name="insertionMode">The insertion mode specifying whether to add at the start or end of the phase.</param>
        /// <returns>The same builder instance.</returns>
        IResolvePipelineBuilder Use(IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase);

        /// <summary>
        /// Use a set of multiple, ordered middleware instances in a resolve pipeline.
        /// </summary>
        /// <param name="middleware">The set of middleware items to add to the pipelne. The set of middleware must be pre-ordered by phase.</param>
        /// <param name="insertionMode">The insertion mode specifying whether to add at the start or end of the phase.</param>
        /// <returns>The same builder instance.</returns>
        IResolvePipelineBuilder UseRange(IEnumerable<IResolveMiddleware> middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase);

        /// <summary>
        /// Clone this builder, returning a new builder containing the set of middleware already added.
        /// </summary>
        /// <returns>A new builder instance.</returns>
        IResolvePipelineBuilder Clone();
    }
}
