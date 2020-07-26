// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with resolve operation failure events.
    /// </summary>
    public class OperationFailureDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailureDiagnosticData"/> class.
        /// </summary>
        /// <param name="operation">The resolve operation that failed.</param>
        /// <param name="operationException">The exception that caused the operation failure.</param>
        public OperationFailureDiagnosticData(IResolveOperation operation, Exception operationException)
        {
            Operation = operation;
            OperationException = operationException;
        }

        /// <summary>
        /// Gets the resolve operation that failed.
        /// </summary>
        public IResolveOperation Operation { get; private set; }

        /// <summary>
        /// Gets the exception that caused the operation failure.
        /// </summary>
        public Exception OperationException { get; private set; }
    }
}
