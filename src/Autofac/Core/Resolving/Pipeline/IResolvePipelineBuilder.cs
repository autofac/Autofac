// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
