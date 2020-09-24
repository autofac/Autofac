// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines an executable resolve middleware.
    /// </summary>
    public interface IResolveMiddleware
    {
        /// <summary>
        /// Gets the phase of the resolve pipeline at which to execute.
        /// </summary>
        PipelinePhase Phase { get; }

        /// <summary>
        /// Invoked when this middleware is executed as part of an active <see cref="ResolveRequest"/>. The middleware should usually call
        /// the <paramref name="next"/> method in order to continue the pipeline, unless the middleware fully satisfies the request.
        /// </summary>
        /// <param name="context">The context for the resolve request.</param>
        /// <param name="next">The method to invoke to continue the pipeline execution; pass this method the <paramref name="context"/> argument.</param>
        void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next);
    }
}
