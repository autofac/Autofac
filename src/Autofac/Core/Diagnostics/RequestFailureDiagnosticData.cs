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
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Diagnostic data associated with resolve request failure events.
    /// </summary>
    public class RequestFailureDiagnosticData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFailureDiagnosticData"/> class.
        /// </summary>
        /// <param name="operation">The pipeline resolve operation that this request is running within.</param>
        /// <param name="requestContext">The context for the resolve request that failed.</param>
        /// <param name="requestException">The exception that caused the failure.</param>
        public RequestFailureDiagnosticData(ResolveOperationBase operation, ResolveRequestContextBase requestContext, Exception requestException)
        {
            Operation = operation;
            RequestContext = requestContext;
            RequestException = requestException;
        }

        /// <summary>
        /// Gets the pipeline resolve operation that this request is running within.
        /// </summary>
        public ResolveOperationBase Operation { get; private set; }

        /// <summary>
        /// Gets the context for the resolve request that failed.
        /// </summary>
        public ResolveRequestContextBase RequestContext { get; private set; }

        /// <summary>
        /// Gets the exception that caused the failure.
        /// </summary>
        public Exception RequestException { get; private set; }
    }
}
