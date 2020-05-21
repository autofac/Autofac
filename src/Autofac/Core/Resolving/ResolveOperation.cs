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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// A <see cref="ResolveOperation"/> is a component context that sequences and monitors the multiple
    /// activations that go into producing a single requested object graph.
    /// </summary>
    internal sealed class ResolveOperation : IPipelineResolveOperation
    {
        private readonly Stack<IResolveRequestContext> _requestStack;
        private readonly IResolvePipelineTracer? _pipelineTracer;
        private List<ResolveRequestContext> _successfulRequests;
        private bool _ended;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperation"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope)
            : this(mostNestedLifetimeScope, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperation"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        /// <param name="pipelineTracer">A pipeline tracer for the operation.</param>
        public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope, IResolvePipelineTracer? pipelineTracer)
        {
            CurrentScope = mostNestedLifetimeScope;
            TracingId = this;
            IsTopLevelOperation = true;
            _pipelineTracer = pipelineTracer;

            _requestStack = new Stack<IResolveRequestContext>();
            _successfulRequests = new List<ResolveRequestContext>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperation"/> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope">The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.</param>
        /// <param name="pipelineTracer">An optional pipeline tracer.</param>
        /// <param name="parentOperation">A parent resolve operation, used to maintain tracing between related operations.</param>
        public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope, IResolvePipelineTracer? pipelineTracer, IPipelineResolveOperation parentOperation)
        {
            CurrentScope = mostNestedLifetimeScope;
            TracingId = parentOperation.TracingId;
            _pipelineTracer = pipelineTracer;

            _requestStack = new Stack<IResolveRequestContext>();
            _successfulRequests = new List<ResolveRequestContext>();
        }

        public IResolveRequestContext? ActiveRequestContext { get; set; }

        public ISharingLifetimeScope CurrentScope { get; set;  }

        public IComponentRegistry ComponentRegistry => CurrentScope.ComponentRegistry;

        public IEnumerable<IResolveRequestContext> InProgressRequests => _requestStack;

        public ResolveRequest? InitiatingRequest { get; private set; }

        public int RequestDepth { get; private set; }

        Stack<IResolveRequestContext> IPipelineResolveOperation.RequestStack => _requestStack;

        public ITracingIdentifer TracingId { get; }

        public bool IsTopLevelOperation { get; }

        public event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

        public event EventHandler<ResolveRequestBeginningEventArgs>? ResolveRequestBeginning;

        /// <summary>
        /// Execute the complete resolve operation.
        /// </summary>
        /// <param name="request">The resolution context.</param>
        [SuppressMessage("CA1031", "CA1031", Justification = "General exception gets rethrown in a DependencyResolutionException.")]
        public object Execute(ResolveRequest request)
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

        /// <summary>
        /// Continue building the object graph by instantiating <paramref name="request"/> in the
        /// current <paramref name="currentOperationScope"/>.
        /// </summary>
        /// <param name="currentOperationScope">The current scope of the operation.</param>
        /// <param name="request">The resolve request.</param>
        /// <returns>The resolved instance.</returns>
        /// <exception cref="ArgumentNullException"/>
        public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request)
        {
            if (_ended) throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);

            // Resolve pipeline from the registration.
            var registrationPipeline = request.Registration.ResolvePipeline;

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

                // Invoke the pipeline.
                registrationPipeline.Invoke(requestContext);

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
                if (_requestStack.Count == 0)
                {
                    CompleteRequests();
                }

                RequestDepth--;
            }

            return requestContext.Instance;
        }

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
