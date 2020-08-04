// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with middleware events.
    /// </summary>
    public class MiddlewareDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareDiagnosticData"/> class.
        /// </summary>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that is running.</param>
        public MiddlewareDiagnosticData(ResolveRequestContext requestContext, IResolveMiddleware middleware)
        {
            RequestContext = requestContext;
            Middleware = middleware;
        }

        /// <summary>
        /// Gets the context for the resolve request that is running.
        /// </summary>
        public ResolveRequestContext RequestContext { get; private set; }

        /// <summary>
        /// Gets the middleware that is running.
        /// </summary>
        public IResolveMiddleware Middleware { get; private set; }
    }
}
