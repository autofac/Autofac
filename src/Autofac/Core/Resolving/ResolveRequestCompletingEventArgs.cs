// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Fired when a resolve request is starting.
    /// </summary>
    public class ResolveRequestCompletingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestCompletingEventArgs"/> class.
        /// </summary>
        /// <param name="requestContext">The resolve request context that is completing.</param>
        public ResolveRequestCompletingEventArgs(ResolveRequestContext requestContext)
        {
            if (requestContext is null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }

            RequestContext = requestContext;
        }

        /// <summary>
        /// Gets the instance lookup operation that is beginning.
        /// </summary>
        public ResolveRequestContext RequestContext { get; }
    }
}
