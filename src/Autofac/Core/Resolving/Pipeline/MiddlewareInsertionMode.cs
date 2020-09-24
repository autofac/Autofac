// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Provides the modes of insertion when adding middleware to an <see cref="IResolvePipelineBuilder"/>.
    /// </summary>
    public enum MiddlewareInsertionMode
    {
        /// <summary>
        /// The new middleware should be added at the end of the middleware's declared phase. The added middleware will run after any middleware already added
        /// at the same phase.
        /// </summary>
        EndOfPhase,

        /// <summary>
        /// The new middleware should be added at the beginning of the middleware's declared phase. The added middleware will run before any middleware
        /// already added at the same phase.
        /// </summary>
        StartOfPhase,
    }
}
