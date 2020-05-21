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

using System.Collections.Generic;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving.Middleware;

namespace Autofac.Core.Resolving.Pipeline
{
    public interface IPipelineResolveOperation : IResolveOperation, ITracingIdentifer
    {
        /// <summary>
        /// Gets the active resolve request.
        /// </summary>
        IResolveRequestContext? ActiveRequestContext { get; }

        /// <summary>
        /// Gets the set of all in-progress requests on the request stack.
        /// </summary>
        IEnumerable<IResolveRequestContext> InProgressRequests { get; }

        /// <summary>
        /// Gets the tracing identifier for the operation.
        /// </summary>
        ITracingIdentifer TracingId { get; }

        /// <summary>
        /// Gets the current request depth.
        /// </summary>
        int RequestDepth { get; }

        /// <summary>
        /// Gets a value indicating whether this operation is a top-level operation (as opposed to one initiated from inside an existing operation).
        /// </summary>
        bool IsTopLevelOperation { get; }

        /// <summary>
        /// Gets the <see cref="ResolveRequest"/> that initiated the operation. Other nested requests may have been issued as a result of this one.
        /// </summary>
        ResolveRequest? InitiatingRequest { get; }

        /// <summary>
        /// Gets the modifiable active request stack.
        /// </summary>
        /// <remarks>
        /// Don't want this exposed to the outside world, but we do want it available in the <see cref="CircularDependencyDetectorMiddleware"/>,
        /// hence it's internal.
        /// </remarks>
        internal Stack<IResolveRequestContext> RequestStack { get; }
    }
}
