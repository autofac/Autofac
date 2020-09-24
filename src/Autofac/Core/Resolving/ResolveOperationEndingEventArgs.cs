// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Describes the commencement of a new resolve operation.
    /// </summary>
    public sealed class ResolveOperationEndingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationEndingEventArgs"/> class.
        /// </summary>
        /// <param name="resolveOperation">The resolve operation that is ending.</param>
        /// <param name="exception">If included, the exception causing the operation to end; otherwise, null.</param>
        public ResolveOperationEndingEventArgs(IResolveOperation resolveOperation, Exception? exception = null)
        {
            ResolveOperation = resolveOperation;
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception causing the operation to end, or null.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the resolve operation that is ending.
        /// </summary>
        public IResolveOperation ResolveOperation { get; }
    }
}
