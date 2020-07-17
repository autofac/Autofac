// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Diagnostics
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
