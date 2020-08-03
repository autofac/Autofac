// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with resolve request failure events.
    /// </summary>
    public class RequestFailureDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFailureDiagnosticData"/> class.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        /// <param name="requestException">The exception that caused the failure.</param>
        public RequestFailureDiagnosticData(IResolveOperation operation, ResolveRequestContext requestContext, Exception requestException)
        {
            Operation = operation;
            RequestContext = requestContext;
            RequestException = requestException;
        }

        /// <summary>
        /// Gets the pipeline resolve operation that this request is running within.
        /// </summary>
        public IResolveOperation Operation { get; private set; }

        /// <summary>
        /// Gets the context for the resolve request that failed.
        /// </summary>
        public ResolveRequestContext RequestContext { get; private set; }

        /// <summary>
        /// Gets the exception that caused the failure.
        /// </summary>
        public Exception RequestException { get; private set; }
    }
}
