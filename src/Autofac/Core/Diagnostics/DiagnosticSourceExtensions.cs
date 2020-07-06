using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    // TODO: Create a common DiagnosticSource instance (a DiagnosticListener) that will be used for handling all events. This should be what gets passed from parent to child instead of a tracer object.
    // https://github.com/dotnet/aspnetcore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/WebHostBuilder.cs
    // TODO: Each child lifetime scope should write to the DiagnosticSource using the same event IDs. Maybe we need this to be an extension method class on DiagnosticSource? ASP.NET has a wrapper class instead.
    // https://github.com/dotnet/aspnetcore/blob/16be9a264e48560e10a3ee9683ecaed342d4ca11/src/Hosting/Hosting/src/Internal/HostingApplication.cs#L29
    // TODO: Write messages to the DiagnosticSource if it's enabled for different events.
    // https://github.com/dotnet/aspnetcore/blob/c97a0020d8bab6d895bf821f6e47ee8722aa17d5/src/Hosting/Hosting/src/Internal/HostingApplicationDiagnostics.cs
    // https://github.com/dotnet/aspnetcore/blob/28157e62597bf0e043bc7e937e44c5ec81946b83/src/Middleware/MiddlewareAnalysis/src/AnalysisMiddleware.cs
    // TODO: Update the default diagnostic tracer so it uses a DiagnosticListener subscription.
    // https://andrewlock.net/logging-using-diagnosticsource-in-asp-net-core/

    /// <summary>
    /// Extension methods for writing diagnostic messages.
    /// </summary>
    internal static class DiagnosticSourceExtensions
    {
        private const string MiddlewareEntryKey = "Autofac.Middleware.Entry";
        private const string MiddlewareFailureKey = "Autofac.Middleware.Failure";
        private const string MiddlewareSuccessKey = "Autofac.Middleware.Success";
        private const string OperationFailureKey = "Autofac.Operation.Failure";
        private const string OperationStartKey = "Autofac.Operation.Start";
        private const string OperationSuccessKey = "Autofac.Operation.Success";
        private const string RequestFailureKey = "Autofac.Request.Failure";
        private const string RequestStartKey = "Autofac.Request.Start";
        private const string RequestSuccessKey = "Autofac.Request.Success";

        /// <summary>
        /// Determines if diagnostics for middleware events is enabled.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to check for diagnostic settings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MiddlewareDiagnosticsEnabled(this DiagnosticSource diagnosticSource)
        {
            return diagnosticSource.IsEnabled(MiddlewareEntryKey) || diagnosticSource.IsEnabled(MiddlewareSuccessKey) || diagnosticSource.IsEnabled(MiddlewareFailureKey);
        }

        /// <summary>
        /// Invoked when an individual middleware item is about to execute (just before the <see cref="IResolveMiddleware.Execute"/> method executes).
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that is about to run.</param>
        public static void MiddlewareEntry(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequestContextBase requestContext, IResolveMiddleware middleware)
        {
            if (diagnosticSource.IsEnabled(MiddlewareEntryKey))
            {
                diagnosticSource.Write(MiddlewareEntryKey, new { operation, requestContext, middleware });
            }
        }

        /// <summary>
        /// Invoked when an individual middleware item has finished executing (when the <see cref="IResolveMiddleware.Execute"/> method returns).
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that just ran.</param>
        /// <param name="succeeded">
        /// Indicates whether the given middleware succeeded. The exception that
        /// caused the middleware to fail is not available here, but will be
        /// available in the next request failure event.
        /// </param>
        public static void MiddlewareExit(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequestContextBase requestContext, IResolveMiddleware middleware, bool succeeded)
        {
            if (succeeded && diagnosticSource.IsEnabled(MiddlewareSuccessKey))
            {
                diagnosticSource.Write(MiddlewareSuccessKey, new { operation, requestContext, middleware });
            }
            else if (!succeeded && diagnosticSource.IsEnabled(MiddlewareFailureKey))
            {
                diagnosticSource.Write(MiddlewareFailureKey, new { operation, requestContext, middleware });
            }
        }

        /// <summary>
        /// Determines if diagnostics for operation events is enabled.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to check for diagnostic settings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool OperationDiagnosticsEnabled(this DiagnosticSource diagnosticSource)
        {
            return diagnosticSource.IsEnabled(OperationStartKey) || diagnosticSource.IsEnabled(OperationSuccessKey) || diagnosticSource.IsEnabled(OperationFailureKey);
        }

        /// <summary>
        /// Invoked at operation start.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that is about to run.</param>
        /// <param name="initiatingRequest">The request that is responsible for starting this operation.</param>
        /// <remarks>
        /// A single operation can in turn invoke other full operations (as opposed to requests). Check <see cref="ResolveOperationBase.IsTopLevelOperation"/>
        /// to know if you're looking at the entry operation.
        /// </remarks>
        public static void OperationStart(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequest initiatingRequest)
        {
            if (diagnosticSource.IsEnabled(OperationStartKey))
            {
                diagnosticSource.Write(OperationStartKey, new { operation, initiatingRequest });
            }
        }

        /// <summary>
        /// Invoked when a resolve operation fails.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The resolve operation that failed.</param>
        /// <param name="operationException">The exception that caused the operation failure.</param>
        public static void OperationFailure(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, Exception operationException)
        {
            if (diagnosticSource.IsEnabled(OperationStartKey))
            {
                diagnosticSource.Write(OperationStartKey, new { operation, operationException });
            }
        }

        /// <summary>
        /// Invoked when a resolve operation succeeds. You can check whether this operation was the top-level entry operation using
        /// <see cref="ResolveOperationBase.IsTopLevelOperation"/>.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The resolve operation that succeeded.</param>
        /// <param name="resolvedInstance">The resolved instance providing the requested service.</param>
        public static void OperationSuccess(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, object resolvedInstance)
        {
            if (diagnosticSource.IsEnabled(OperationStartKey))
            {
                diagnosticSource.Write(OperationStartKey, new { operation, resolvedInstance });
            }
        }

        /// <summary>
        /// Invoked at the start of a single resolve request initiated from within an operation.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request  that is about to start.</param>
        public static void RequestStart(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequestContextBase requestContext)
        {
            if (diagnosticSource.IsEnabled(RequestStartKey))
            {
                diagnosticSource.Write(RequestStartKey, new { operation, requestContext });
            }
        }

        /// <summary>
        /// Invoked when a resolve request fails.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        /// <param name="requestException">The exception that caused the failure.</param>
        public static void RequestFailure(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequestContextBase requestContext, Exception requestException)
        {
            if (diagnosticSource.IsEnabled(RequestFailureKey))
            {
                diagnosticSource.Write(RequestFailureKey, new { operation, requestContext, requestException });
            }
        }

        /// <summary>
        /// Invoked when a resolve request succeeds.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        public static void RequestSuccess(this DiagnosticSource diagnosticSource, ResolveOperationBase operation, ResolveRequestContextBase requestContext)
        {
            if (diagnosticSource.IsEnabled(RequestSuccessKey))
            {
                diagnosticSource.Write(RequestSuccessKey, new { operation, requestContext });
            }
        }
    }
}
