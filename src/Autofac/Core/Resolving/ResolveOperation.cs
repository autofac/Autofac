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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// A <see cref="ResolveOperation"/> is a component context that sequences and monitors the multiple
    /// activations that go into producing a single requested object graph.
    /// </summary>
    internal sealed class ResolveOperation : IDependencyTrackingResolveOperation
    {
        private const int SuccessListInitialCapacity = 32;
        private bool _ended;

        private readonly List<DefaultResolveRequestContext> _successfulRequests =
            new List<DefaultResolveRequestContext>(SuccessListInitialCapacity);

        private int _nextCompleteSuccessfulRequestStartPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperation" /> class.
        /// </summary>
        /// <param name="mostNestedLifetimeScope"> The most nested scope in which to begin the operation. The operation
        /// can move upward to less nested scopes as components with wider sharing scopes are activated.
        /// </param>
        /// <param name="diagnosticSource">
        /// The <see cref="System.Diagnostics.DiagnosticListener" /> to which trace events should be written.
        /// </param>
        public ResolveOperation(
            ISharingLifetimeScope mostNestedLifetimeScope,
            DiagnosticListener diagnosticSource)
        {
            CurrentScope = mostNestedLifetimeScope ?? throw new ArgumentNullException(nameof(mostNestedLifetimeScope));
            DiagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        }

        /// <summary>
        /// Execute the complete resolve operation.
        /// </summary>
        /// <param name="request">The resolution context.</param>
        public object Execute(ResolveRequest request)
        {
            return ExecuteOperation(request);
        }

        /// <summary>
        /// Gets the active resolve request.
        /// </summary>
        public ResolveRequestContext? ActiveRequestContext { get; private set; }

        /// <summary>
        /// Gets the current lifetime scope of the operation; based on the most recently executed request.
        /// </summary>
        public ISharingLifetimeScope CurrentScope { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<ResolveRequestContext> InProgressRequests => RequestStack;

        /// <summary>
        /// Gets the <see cref="System.Diagnostics.DiagnosticListener" /> for the operation.
        /// </summary>
        public DiagnosticListener DiagnosticSource { get; }

        /// <summary>
        /// Gets the current request depth.
        /// </summary>
        public int RequestDepth { get; private set; }

        /// <summary>
        /// Gets the <see cref="ResolveRequest" /> that initiated the operation. Other nested requests may have been
        /// issued as a result of this one.
        /// </summary>
        public ResolveRequest? InitiatingRequest { get; private set; }

        /// <inheritdoc />
        public event EventHandler<ResolveRequestBeginningEventArgs>? ResolveRequestBeginning;

        /// <inheritdoc />
        public event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

        /// <summary>
        /// Enter a new dependency chain block where subsequent requests inside the operation are allowed to repeat
        /// registrations from before the block.
        /// </summary>
        /// <returns>A disposable that should be disposed to exit the block.</returns>
        public IDisposable EnterNewDependencyDetectionBlock() => RequestStack.EnterSegment();

        /// <inheritdoc/>
        public SegmentedStack<ResolveRequestContext> RequestStack { get; } = new SegmentedStack<ResolveRequestContext>();

        /// <inheritdoc />
        public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (_ended)
            {
                throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);
            }

            // Create a new request context.
            var requestContext = new DefaultResolveRequestContext(this, request, currentOperationScope, DiagnosticSource);

            // Raise our request-beginning event.
            var handler = ResolveRequestBeginning;
            handler?.Invoke(this, new ResolveRequestBeginningEventArgs(requestContext));

            RequestDepth++;

            // Track the last active request and scope in the call stack.
            ResolveRequestContext? lastActiveRequest = ActiveRequestContext;
            var lastScope = CurrentScope;

            ActiveRequestContext = requestContext;
            CurrentScope = currentOperationScope;

            try
            {
                // Same basic flow in if/else, but doing a one-time check for diagnostics
                // and choosing the "diagnostics enabled" version vs. the more common
                // "no diagnostics enabled" path: hot-path optimization.
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.RequestStart(this, requestContext);
                    InvokePipeline(request, requestContext);
                    DiagnosticSource.RequestSuccess(this, requestContext);
                }
                else
                {
                    InvokePipeline(request, requestContext);
                }
            }
            catch (Exception ex)
            {
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.RequestFailure(this, requestContext, ex);
                }

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

            // InvokePipeline throws if the instance is null but
            // analyzers don't pick that up and get mad.
            return requestContext.Instance!;
        }

        /// <summary>
        /// Invoke this method to execute the operation for a given request.
        /// </summary>
        /// <param name="request">The resolve request.</param>
        /// <returns>The resolved instance.</returns>
        private object ExecuteOperation(ResolveRequest request)
        {
            object result;

            try
            {
                InitiatingRequest = request;
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.OperationStart(this, request);
                }

                result = GetOrCreateInstance(CurrentScope, request);
            }
            catch (ObjectDisposedException disposeException)
            {
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.OperationFailure(this, disposeException);
                }

                throw;
            }
            catch (DependencyResolutionException dependencyResolutionException)
            {
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.OperationFailure(this, dependencyResolutionException);
                }

                End(dependencyResolutionException);
                throw;
            }
            catch (Exception exception)
            {
                End(exception);
                if (DiagnosticSource.IsEnabled())
                {
                    DiagnosticSource.OperationFailure(this, exception);
                }

                throw new DependencyResolutionException(ResolveOperationResources.ExceptionDuringResolve, exception);
            }
            finally
            {
                ResetSuccessfulRequests();
            }

            End();

            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.OperationSuccess(this, result);
            }

            return result;
        }

        /// <summary>
        /// Basic pipeline invocation steps used when retrieving an instance. Isolated
        /// to enable it to be optionally surrounded with diagnostics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokePipeline(ResolveRequest request, DefaultResolveRequestContext requestContext)
        {
            request.ResolvePipeline.Invoke(requestContext);
            if (requestContext.Instance == null)
            {
                throw new DependencyResolutionException(ResolveOperationResources.PipelineCompletedWithNoInstance);
            }

            _successfulRequests.Add(requestContext);
        }

        private void CompleteRequests()
        {
            var completed = _successfulRequests;
            var count = completed.Count;
            var startPosition = _nextCompleteSuccessfulRequestStartPos;
            ResetSuccessfulRequests();

            for (var i = startPosition; i < count; i++)
            {
                completed[i].CompleteRequest();
            }
        }

        private void ResetSuccessfulRequests()
        {
            _nextCompleteSuccessfulRequestStartPos = _successfulRequests.Count;
        }

        private void End(Exception? exception = null)
        {
            if (_ended)
            {
                return;
            }

            _ended = true;
            var handler = CurrentOperationEnding;
            handler?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
        }
    }
}
