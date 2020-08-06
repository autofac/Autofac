// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Extension methods for writing diagnostic messages.
    /// </summary>
    internal static class DiagnosticSourceExtensions
    {
        /// <summary>
        /// Writes a diagnostic event indicating an individual middleware item is about to execute (just before the <see cref="IResolveMiddleware.Execute"/> method executes).
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that is about to run.</param>
        public static void MiddlewareStart(this DiagnosticListener diagnosticSource, ResolveRequestContext requestContext, IResolveMiddleware middleware)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.MiddlewareStart))
            {
                diagnosticSource.Write(DiagnosticEventKeys.MiddlewareStart, new MiddlewareDiagnosticData(requestContext, middleware));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating an individual middleware item has finished in an error state (when the <see cref="IResolveMiddleware.Execute"/> method returns).
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that just ran.</param>
        public static void MiddlewareFailure(this DiagnosticListener diagnosticSource, ResolveRequestContext requestContext, IResolveMiddleware middleware)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.MiddlewareFailure))
            {
                diagnosticSource.Write(DiagnosticEventKeys.MiddlewareFailure, new MiddlewareDiagnosticData(requestContext, middleware));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating an individual middleware item has finished successfully (when the <see cref="IResolveMiddleware.Execute"/> method returns).
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="requestContext">The context for the resolve request that is running.</param>
        /// <param name="middleware">The middleware that just ran.</param>
        public static void MiddlewareSuccess(this DiagnosticListener diagnosticSource, ResolveRequestContext requestContext, IResolveMiddleware middleware)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.MiddlewareSuccess))
            {
                diagnosticSource.Write(DiagnosticEventKeys.MiddlewareSuccess, new MiddlewareDiagnosticData(requestContext, middleware));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve operation has started.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that is about to run.</param>
        /// <param name="initiatingRequest">The request that is responsible for starting this operation.</param>
        public static void OperationStart(this DiagnosticListener diagnosticSource, IResolveOperation operation, ResolveRequest initiatingRequest)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.OperationStart))
            {
                diagnosticSource.Write(DiagnosticEventKeys.OperationStart, new OperationStartDiagnosticData(operation, initiatingRequest));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve operation has failed.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The resolve operation that failed.</param>
        /// <param name="operationException">The exception that caused the operation failure.</param>
        public static void OperationFailure(this DiagnosticListener diagnosticSource, IResolveOperation operation, Exception operationException)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.OperationFailure))
            {
                diagnosticSource.Write(DiagnosticEventKeys.OperationFailure, new OperationFailureDiagnosticData(operation, operationException));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve operation has succeeded.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The resolve operation that succeeded.</param>
        /// <param name="resolvedInstance">The resolved instance providing the requested service.</param>
        public static void OperationSuccess(this DiagnosticListener diagnosticSource, IResolveOperation operation, object resolvedInstance)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.OperationSuccess))
            {
                diagnosticSource.Write(DiagnosticEventKeys.OperationSuccess, new OperationSuccessDiagnosticData(operation, resolvedInstance));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve request has started inside an operation.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that is about to start.</param>
        public static void RequestStart(this DiagnosticListener diagnosticSource, IResolveOperation operation, ResolveRequestContext requestContext)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.RequestStart))
            {
                diagnosticSource.Write(DiagnosticEventKeys.RequestStart, new RequestDiagnosticData(operation, requestContext));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve request inside an operation has failed.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        /// <param name="requestException">The exception that caused the failure.</param>
        public static void RequestFailure(this DiagnosticListener diagnosticSource, IResolveOperation operation, ResolveRequestContext requestContext, Exception requestException)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.RequestFailure))
            {
                diagnosticSource.Write(DiagnosticEventKeys.RequestFailure, new RequestFailureDiagnosticData(operation, requestContext, requestException));
            }
        }

        /// <summary>
        /// Writes a diagnostic event indicating a resolve request inside an operation has succeeded.
        /// </summary>
        /// <param name="diagnosticSource">The diagnostic source to which events will be written.</param>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        public static void RequestSuccess(this DiagnosticListener diagnosticSource, IResolveOperation operation, ResolveRequestContext requestContext)
        {
            if (diagnosticSource.IsEnabled(DiagnosticEventKeys.RequestSuccess))
            {
                diagnosticSource.Write(DiagnosticEventKeys.RequestSuccess, new RequestDiagnosticData(operation, requestContext));
            }
        }
    }
}
