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

using System;
using System.Collections.Generic;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving.Middleware;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines the base properties and behaviour of a resolve operation.
    /// </summary>
    public abstract class ResolveOperationBase : IResolveOperation, ITracingIdentifer
    {
        private bool _ended;
        private IResolvePipelineTracer? _pipelineTracer;
        private List<ResolveRequestContext> _successfulRequests = new List<ResolveRequestContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationBase"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        protected ResolveOperationBase(ISharingLifetimeScope mostNestedLifetimeScope)
            : this(mostNestedLifetimeScope, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationBase"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        /// <param name="pipelineTracer">A pipeline tracer for the operation.</param>
        protected ResolveOperationBase(ISharingLifetimeScope mostNestedLifetimeScope, IResolvePipelineTracer? pipelineTracer)
        {
            TracingId = this;
            CurrentScope = mostNestedLifetimeScope;
            _pipelineTracer = pipelineTracer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationBase"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        /// <param name="pipelineTracer">A pipeline tracer for the operation.</param>
        /// <param name="tracingId">A tracing ID for the operation.</param>
        protected ResolveOperationBase(ISharingLifetimeScope mostNestedLifetimeScope, IResolvePipelineTracer? pipelineTracer, ITracingIdentifer tracingId)
            : this(mostNestedLifetimeScope, pipelineTracer)
        {
            TracingId = tracingId;
        }

        /// <summary>
        /// Gets the active resolve request.
        /// </summary>
        public ResolveRequestContextBase? ActiveRequestContext { get; private set; }

        public ISharingLifetimeScope CurrentScope { get; private set; }

        /// <summary>
        /// Gets the set of all in-progress requests on the request stack.
        /// </summary>
        public IEnumerable<ResolveRequestContextBase> InProgressRequests => RequestStack;

        /// <summary>
        /// Gets the tracing identifier for the operation.
        /// </summary>
        public ITracingIdentifer TracingId { get; }

        /// <summary>
        /// Gets or sets the current request depth.
        /// </summary>
        public int RequestDepth { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this operation is a top-level operation (as opposed to one initiated from inside an existing operation).
        /// </summary>
        public bool IsTopLevelOperation { get; }

        /// <summary>
        /// Gets or sets the <see cref="ResolveRequest"/> that initiated the operation. Other nested requests may have been issued as a result of this one.
        /// </summary>
        public ResolveRequest? InitiatingRequest { get; protected set; }

        /// <summary>
        /// Gets the modifiable active request stack.
        /// </summary>
        /// <remarks>
        /// Don't want this exposed to the outside world, but we do want it available in the <see cref="CircularDependencyDetectorMiddleware"/>,
        /// hence it's internal.
        /// </remarks>
        internal Stack<ResolveRequestContextBase> RequestStack { get; } = new Stack<ResolveRequestContextBase>();

        /// <inheritdoc />
        public event EventHandler<ResolveRequestBeginningEventArgs>? ResolveRequestBeginning;

        /// <inheritdoc />
        public event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

        /// <summary>
        /// Invoke this method to execute the operation for a given request.
        /// </summary>
        /// <param name="request">The resolve request.</param>
        /// <returns>The resolved instance.</returns>
        protected object ExecuteOperation(ResolveRequest request)
        {
            object result;

            try
            {
                InitiatingRequest = request;

                _pipelineTracer?.OperationStart(this, request);

                result = GetOrCreateInstance(CurrentScope, request);
            }
            catch (ObjectDisposedException disposeException)
            {
                _pipelineTracer?.OperationFailure(this, disposeException);

                throw;
            }
            catch (DependencyResolutionException dependencyResolutionException)
            {
                _pipelineTracer?.OperationFailure(this, dependencyResolutionException);
                End(dependencyResolutionException);
                throw;
            }
            catch (Exception exception)
            {
                End(exception);
                _pipelineTracer?.OperationFailure(this, exception);
                throw new DependencyResolutionException(ResolveOperationResources.ExceptionDuringResolve, exception);
            }
            finally
            {
                ResetSuccessfulRequests();
            }

            End();

            _pipelineTracer?.OperationSuccess(this, result);

            return result;
        }

        /// <inheritdoc />
        public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request)
        {
            if (_ended) throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);

            // Create a new request context.
            var requestContext = new ResolveRequestContext(this, request, currentOperationScope, _pipelineTracer);

            // Raise our request-beginning event.
            var handler = ResolveRequestBeginning;
            handler?.Invoke(this, new ResolveRequestBeginningEventArgs(requestContext));

            RequestDepth++;

            // Track the last active request and scope in the call stack.
            var lastActiveRequest = ActiveRequestContext;
            var lastScope = CurrentScope;

            ActiveRequestContext = requestContext;
            CurrentScope = currentOperationScope;

            try
            {
                _pipelineTracer?.RequestStart(this, requestContext);

                ExecuteRequest(requestContext);

                if (requestContext.Instance == null)
                {
                    // No exception, but was null; this shouldn't happen.
                    throw new DependencyResolutionException(ResolveOperationResources.PipelineCompletedWithNoInstance);
                }

                _successfulRequests.Add(requestContext);
                _pipelineTracer?.RequestSuccess(this, requestContext);
            }
            catch (Exception ex)
            {
                _pipelineTracer?.RequestFailure(this, requestContext, ex);
                throw;
            }
            finally
            {
                ActiveRequestContext = lastActiveRequest;
                CurrentScope = lastScope;

                // Raise the appropriate completion events.
                if (RequestStack.Count == 0)
                {
                    CompleteRequests();
                }

                RequestDepth--;
            }

            return requestContext.Instance;
        }

        /// <summary>
        /// An implementation must implement this member to execute the specified request context.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        protected abstract void ExecuteRequest(ResolveRequestContextBase requestContext);

        private void CompleteRequests()
        {
            var completed = _successfulRequests;
            int count = completed.Count;
            ResetSuccessfulRequests();

            for (int i = 0; i < count; i++)
            {
                completed[i].Complete();
            }
        }

        private void ResetSuccessfulRequests()
        {
            _successfulRequests = new List<ResolveRequestContext>();
        }

        private void End(Exception? exception = null)
        {
            if (_ended) return;

            _ended = true;
            var handler = CurrentOperationEnding;
            handler?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
        }
    }
}
