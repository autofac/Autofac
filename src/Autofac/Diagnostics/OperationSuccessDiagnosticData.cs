// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with resolve operation success events.
    /// </summary>
    public class OperationSuccessDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationSuccessDiagnosticData"/> class.
        /// </summary>
        /// <param name="operation">The resolve operation that succeeded.</param>
        /// <param name="resolvedInstance">The resolved instance providing the requested service.</param>
        public OperationSuccessDiagnosticData(IResolveOperation operation, object resolvedInstance)
        {
            Operation = operation;
            ResolvedInstance = resolvedInstance;
        }

        /// <summary>
        /// Gets the resolve operation that succeeded.
        /// </summary>
        public IResolveOperation Operation { get; private set; }

        /// <summary>
        /// Gets the resolved instance providing the requested service.
        /// </summary>
        public object ResolvedInstance { get; private set; }
    }
}
