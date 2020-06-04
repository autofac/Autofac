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

using Autofac.Core.Diagnostics;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Context area for a resolve request.
    /// </summary>
    internal sealed class ResolveRequestContext : ResolveRequestContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestContext"/> class.
        /// </summary>
        /// <param name="owningOperation">The owning resolve operation.</param>
        /// <param name="request">The initiating resolve request.</param>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="tracer">An optional tracer.</param>
        internal ResolveRequestContext(
            ResolveOperationBase owningOperation,
            ResolveRequest request,
            ISharingLifetimeScope scope,
            IResolvePipelineTracer? tracer)
            : base(owningOperation, request, scope, tracer)
        {
        }

        /// <inheritdoc/>
        public override object ResolveComponent(ResolveRequest request)
        {
            return Operation.GetOrCreateInstance(ActivationScope, request);
        }

        /// <inheritdoc/>
        public override object ResolveComponentWithNewOperation(ResolveRequest request)
        {
            // Create a new operation, with the current ActivationScope and Tracer.
            // Pass in the current operation as a tracing reference.
            var operation = new ResolveOperation(ActivationScope, Tracer, Operation);
            return operation.Execute(request);
        }

        /// <summary>
        /// Completes the request context; invokes any event handlers.
        /// </summary>
        public void Complete()
        {
            // Let the base class raise events.
            CompleteRequest();
        }
    }
}
