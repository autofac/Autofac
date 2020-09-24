// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// The interface indicating that an <see cref="IResolveOperation" /> supports dependency tracking. Required by middleware that must understand the dependency tree.
    /// </summary>
    public interface IDependencyTrackingResolveOperation : IResolveOperation
    {
        /// <summary>
        /// Enter a new dependency chain block where subsequent requests inside the operation are allowed to repeat
        /// registrations from before the block.
        /// </summary>
        /// <returns>A disposable that should be disposed to exit the block.</returns>
        IDisposable EnterNewDependencyDetectionBlock();

        /// <summary>
        /// Gets the modifiable active request stack.
        /// </summary>
        /// <remarks>
        /// Don't want this exposed to the outside world, but we do want it available in the
        /// <see cref="CircularDependencyDetectorMiddleware" />,
        /// hence it's internal.
        /// </remarks>
        SegmentedStack<ResolveRequestContext> RequestStack { get; }
    }
}
