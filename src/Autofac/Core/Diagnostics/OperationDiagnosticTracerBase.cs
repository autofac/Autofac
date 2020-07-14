// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
