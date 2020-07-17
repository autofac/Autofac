// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Base class for tracers that require all operations for logical operation tracing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived classes will be subscribed to all Autofac diagnostic events
    /// and will raise an <see cref="OperationDiagnosticTracerBase.OperationCompleted"/>
    /// event when a logical operation has finished and trace data is available.
    /// </para>
    /// </remarks>
    public abstract class OperationDiagnosticTracerBase : DiagnosticTracerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationDiagnosticTracerBase"/> class
        /// and enables all subscriptions.
        /// </summary>
        protected OperationDiagnosticTracerBase()
        {
            EnableAll();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationDiagnosticTracerBase"/> class
        /// and enables a specified set of subscriptions.
        /// </summary>
        /// <param name="subscriptions">
        /// The set of subscriptions that should be enabled by default.
        /// </param>
        protected OperationDiagnosticTracerBase(IEnumerable<string> subscriptions)
        {
            if (subscriptions == null)
            {
                throw new ArgumentNullException(nameof(subscriptions));
            }

            foreach (var subscription in subscriptions)
            {
                Enable(subscription);
            }
        }

        /// <summary>
        /// Event raised when a resolve operation completes and trace data is available.
        /// </summary>
        public event EventHandler<OperationTraceCompletedArgs>? OperationCompleted;

        /// <summary>
        /// Gets the number of operations in progress being traced.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> with the number of trace IDs associated
        /// with in-progress operations being traced by this tracer.
        /// </value>
        public abstract int OperationsInProgress { get; }

        /// <summary>
        /// Invokes the <see cref="OperationCompleted"/> event.
        /// </summary>
        /// <param name="args">
        /// The arguments to provide in the raised event.
        /// </param>
        protected virtual void OnOperationCompleted(OperationTraceCompletedArgs args)
        {
            OperationCompleted?.Invoke(this, args);
        }
    }
}
