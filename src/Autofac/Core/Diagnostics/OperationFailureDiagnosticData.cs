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
using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
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
        public OperationFailureDiagnosticData(ResolveOperationBase operation, Exception operationException)
        {
            Operation = operation;
            OperationException = operationException;
        }

        /// <summary>
        /// Gets the resolve operation that failed.
        /// </summary>
        public ResolveOperationBase Operation { get; private set; }

        /// <summary>
        /// Gets the exception that caused the operation failure.
        /// </summary>
        public Exception OperationException { get; private set; }
    }
}
