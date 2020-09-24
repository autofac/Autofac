// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Describes the commencement of a new resolve operation.
    /// </summary>
    public sealed class ResolveOperationBeginningEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationBeginningEventArgs"/> class.
        /// </summary>
        /// <param name="resolveOperation">The resolve operation that is beginning.</param>
        public ResolveOperationBeginningEventArgs(IResolveOperation resolveOperation)
        {
            ResolveOperation = resolveOperation;
        }

        /// <summary>
        /// Gets the resolve operation that is beginning.
        /// </summary>
        public IResolveOperation ResolveOperation { get; }
    }
}
