using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Defines the interface for a tracer that is invoked by a resolve pipeline during execution. Implement this class if you want
    /// to provide custom trace output or other diagnostic functionality.
    /// </summary>
    /// <remarks>
    /// You can get a 'tracing ID' object from <see cref="IPipelineResolveOperation.TracingId"/> that can be used as a dictionary tracking key
    /// to associate related operations.
    /// </remarks>
    /// <seealso cref="DefaultDiagnosticTracer"/>
    public interface IResolvePipelineTracer
    {
        /// <summary>
        /// Invoked at operation start.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that is about to run.</param>
        /// <param name="initiatingRequest">The request that is responsible for starting this operation.</param>
        /// <remarks>
        /// A single operation can in turn invoke other full operations (as opposed to requests). Check <see cref="IPipelineResolveOperation.IsTopLevelOperation"/>
        /// to know if you're looking at the entry operation.
        /// </remarks>
        void OperationStart(IPipelineResolveOperation operation, ResolveRequest initiatingRequest);

        /// <summary>
        /// Invoked at the start of a single resolve request initiated from within an operation.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request  that is about to start.</param>
        void RequestStart(IPipelineResolveOperation operation, IResolveRequestContext requestContext);

        /// <summary>
        /// Invoked when an individual middleware item is about to execute (just before the <see cref="IResolveMiddleware.Execute"/> method executes).
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that is about to run.</param>
        void MiddlewareEntry(IPipelineResolveOperation operation, IResolveRequestContext requestContext, IResolveMiddleware middleware);

        /// <summary>
        /// Invoked when an individual middleware item has finished executing (when the <see cref="IResolveMiddleware.Execute"/> method returns).
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that just ran.</param>
        /// <param name="succeeded">
        /// Indicates whether the given middleware succeeded.
        /// The exception that caused the middleware to fail is not available here, but will be available in the next <see cref="RequestFailure"/> call.
        /// </param>
        void MiddlewareExit(IPipelineResolveOperation operation, IResolveRequestContext requestContext, IResolveMiddleware middleware, bool succeeded);

        /// <summary>
        /// Invoked when a resolve request fails.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        /// <param name="requestException">The exception that caused the failure.</param>
        void RequestFailure(IPipelineResolveOperation operation, IResolveRequestContext requestContext, Exception requestException);

        /// <summary>
        /// Invoked when a resolve request succeeds.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        void RequestSuccess(IPipelineResolveOperation operation, IResolveRequestContext requestContext);

        /// <summary>
        /// Invoked when a resolve operation fails.
        /// </summary>
        /// <param name="operation">The resolve operation that failed.</param>
        /// <param name="operationException">The exception that caused the operation failure.</param>
        void OperationFailure(IPipelineResolveOperation operation, Exception operationException);

        /// <summary>
        /// Invoked when a resolve operation succeeds. You can check whether this operation was the top-level entry operation using
        /// <see cref="IPipelineResolveOperation.IsTopLevelOperation"/>.
        /// </summary>
        /// <param name="operation">The resolve operation that succeeded.</param>
        /// <param name="resolvedInstance">The resolved instance providing the requested service.</param>
        void OperationSuccess(IPipelineResolveOperation operation, object resolvedInstance);
    }
}
