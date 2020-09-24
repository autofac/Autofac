// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// An <see cref="IResolveOperation"/> is a component context that sequences and monitors the multiple
    /// activations that go into producing a single requested object graph.
    /// </summary>
    public interface IResolveOperation
    {
        /// <summary>
        /// Gets the active resolve request.
        /// </summary>
        ResolveRequestContext? ActiveRequestContext { get; }

        /// <summary>
        /// Gets the current lifetime scope of the operation; based on the most recently executed request.
        /// </summary>
        ISharingLifetimeScope CurrentScope { get; }

        /// <summary>
        /// Gets the set of all in-progress requests on the request stack.
        /// </summary>
        IEnumerable<ResolveRequestContext> InProgressRequests { get; }

        /// <summary>
        /// Gets the <see cref="System.Diagnostics.DiagnosticListener" /> for the operation.
        /// </summary>
        DiagnosticListener DiagnosticSource { get; }

        /// <summary>
        /// Gets the current request depth.
        /// </summary>
        int RequestDepth { get; }

        /// <summary>
        /// Gets the <see cref="ResolveRequest" /> that initiated the operation. Other nested requests may have been
        /// issued as a result of this one.
        /// </summary>
        ResolveRequest? InitiatingRequest { get; }

        /// <summary>
        /// Raised when a resolve request starts.
        /// </summary>
        event EventHandler<ResolveRequestBeginningEventArgs>? ResolveRequestBeginning;

        /// <summary>
        /// Raised when the entire operation is complete.
        /// </summary>
        event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

        /// <summary>
        /// Get or create and share an instance of the requested service in the <paramref name="currentOperationScope"/>.
        /// </summary>
        /// <param name="currentOperationScope">The scope in the hierarchy in which the operation will begin.</param>
        /// <param name="request">The resolve request.</param>
        /// <returns>The component instance.</returns>
        object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request);
    }
}
