// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
