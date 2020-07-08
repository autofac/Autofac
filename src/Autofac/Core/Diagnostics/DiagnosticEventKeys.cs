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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Names of the events raised in diagnostics.
    /// </summary>
    internal static class DiagnosticEventKeys
    {
        /// <summary>
        /// ID for the event raised when middleware starts.
        /// </summary>
        public const string MiddlewareStart = "Autofac.Middleware.Start";

        /// <summary>
        /// ID for the event raised when middleware encounters an error.
        /// </summary>
        public const string MiddlewareFailure = "Autofac.Middleware.Failure";

        /// <summary>
        /// ID for the event raised when middleware exits successfully.
        /// </summary>
        public const string MiddlewareSuccess = "Autofac.Middleware.Success";

        /// <summary>
        /// ID for the event raised when a resolve operation encounters an error.
        /// </summary>
        public const string OperationFailure = "Autofac.Operation.Failure";

        /// <summary>
        /// ID for the event raised when a resolve operation starts.
        /// </summary>
        public const string OperationStart = "Autofac.Operation.Start";

        /// <summary>
        /// ID for the event raised when a resolve operation completes successfully.
        /// </summary>
        public const string OperationSuccess = "Autofac.Operation.Success";

        /// <summary>
        /// ID for the event raised when a resolve request encounters an error.
        /// </summary>
        public const string RequestFailure = "Autofac.Request.Failure";

        /// <summary>
        /// ID for the event raised when a resolve request starts.
        /// </summary>
        public const string RequestStart = "Autofac.Request.Start";

        /// <summary>
        /// ID for the event raised when a resolve request completes successfully.
        /// </summary>
        public const string RequestSuccess = "Autofac.Request.Success";
    }
}
