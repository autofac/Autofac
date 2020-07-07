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
using Autofac.Core.Diagnostics;

namespace Autofac
{
    /// <summary>
    /// Extensions to the lifetime scope to provide convenience methods for tracing.
    /// </summary>
    public static class LifetimeScopeExtensions
    {
        /// <summary>
        /// Enable tracing on this scope, routing trace events to the specified tracer.
        /// All lifetime scopes created from this one will inherit this tracer as well.
        /// </summary>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="newTraceCallback">A callback that will receive the trace output.</param>
        /// <remarks>
        /// Only one tracer is supported at once, so calling this will replace any prior tracer on this scope. Existing nested scopes
        /// will continue to retain their original trace behaviour.
        /// </remarks>
        public static void SubscribeToDiagnostics(this ILifetimeScope scope, DiagnosticTracerBase tracer)
        {
            if (scope is null) throw new ArgumentNullException(nameof(scope));
            if (tracer is null) throw new ArgumentNullException(nameof(tracer));
            scope.DiagnosticSource.Subscribe(tracer, tracer.IsEnabled);
        }

        /// <summary>
        /// Enable tracing on this scope, routing trace events to the specified tracer.
        /// All lifetime scopes created from this one will inherit this tracer as well.
        /// </summary>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="newTraceCallback">A callback that will receive the trace output.</param>
        /// <remarks>
        /// Only one tracer is supported at once, so calling this will replace any prior tracer on this scope. Existing nested scopes
        /// will continue to retain their original trace behaviour.
        /// </remarks>
        public static T SubscribeToDiagnostics<T>(this ILifetimeScope scope)
            where T : DiagnosticTracerBase, new()
        {
            if (scope is null) throw new ArgumentNullException(nameof(scope));
            var tracer = new T();
            scope.SubscribeToDiagnostics(tracer);
            return tracer;
        }
    }
}
