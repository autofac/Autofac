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

using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
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
        public OperationStartDiagnosticData(ResolveOperationBase operation, ResolveRequest initiatingRequest)
        {
            Operation = operation;
            InitiatingRequest = initiatingRequest;
        }

        /// <summary>
        /// Gets the pipeline resolve operation that is about to run.
        /// </summary>
        public ResolveOperationBase Operation { get; private set; }

        /// <summary>
        /// Gets the request that is responsible for starting this operation.
        /// </summary>
        public ResolveRequest InitiatingRequest { get; private set; }
    }
}
