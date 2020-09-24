// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines a declared piece of middleware in a pipeline builder.
    /// </summary>
    internal sealed class MiddlewareDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareDeclaration"/> class.
        /// </summary>
        /// <param name="middleware">The middleware that is encapsulated in this declaration.</param>
        public MiddlewareDeclaration(IResolveMiddleware middleware)
        {
            Middleware = middleware;
            Phase = middleware.Phase;
        }

        /// <summary>
        /// Gets or sets the next node in a pipeline set.
        /// </summary>
        public MiddlewareDeclaration? Next { get; set; }

        /// <summary>
        /// Gets or sets the previous node in a pipeline set.
        /// </summary>
        public MiddlewareDeclaration? Previous { get; set; }

        /// <summary>
        /// Gets the middleware for this declaration.
        /// </summary>
        public IResolveMiddleware Middleware { get; }

        /// <summary>
        /// Gets the declared phase of the middleware.
        /// </summary>
        public PipelinePhase Phase { get; }
    }
}
