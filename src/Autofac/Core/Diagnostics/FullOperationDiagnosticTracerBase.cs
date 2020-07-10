using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Base class for tracers that require all operations for logical operation tracing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived classes will be subscribed to all Autofac diagnostic events
    /// and will raise an <see cref="FullOperationDiagnosticTracerBase.OperationCompleted"/>
    /// event when a logical operation has finished and trace data is available.
    /// </para>
    /// </remarks>
    public abstract class FullOperationDiagnosticTracerBase : DiagnosticTracerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullOperationDiagnosticTracerBase"/> class.
        /// </summary>
        protected FullOperationDiagnosticTracerBase()
        {
            EnableAll();
        }

        /// <inheritdoc/>
        public override void Enable(string diagnosticName)
        {
            // Do nothing. Default is always enabled for everything.
        }

        /// <inheritdoc/>
        public override void Disable(string diagnosticName)
        {
            // Do nothing. Default is always enabled for everything.
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
