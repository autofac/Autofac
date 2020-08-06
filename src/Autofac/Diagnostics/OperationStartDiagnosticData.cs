// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with resolve operation start events.
    /// </summary>
    public class OperationStartDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStartDiagnosticData"/> class.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that is about to run.</param>
        /// <param name="initiatingRequest">The request that is responsible for starting this operation.</param>
        public OperationStartDiagnosticData(IResolveOperation operation, ResolveRequest initiatingRequest)
        {
            Operation = operation;
            InitiatingRequest = initiatingRequest;
        }

        /// <summary>
        /// Gets the pipeline resolve operation that is about to run.
        /// </summary>
        public IResolveOperation Operation { get; private set; }

        /// <summary>
        /// Gets the request that is responsible for starting this operation.
        /// </summary>
        public ResolveRequest InitiatingRequest { get; private set; }
    }
}
