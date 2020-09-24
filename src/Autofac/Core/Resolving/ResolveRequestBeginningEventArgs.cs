// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Fired when a resolve request is starting.
    /// </summary>
    public sealed class ResolveRequestBeginningEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestBeginningEventArgs"/> class.
        /// </summary>
        /// <param name="requestContext">The resolve request context that is starting.</param>
        public ResolveRequestBeginningEventArgs(ResolveRequestContext requestContext)
        {
            RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        /// <summary>
        /// Gets the resolve request that is beginning.
        /// </summary>
        public ResolveRequestContext RequestContext { get; }
    }
}
