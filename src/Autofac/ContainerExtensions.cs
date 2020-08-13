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
using Autofac.Diagnostics;

namespace Autofac
{
    /// <summary>
    /// Extensions to the container to provide convenience methods for tracing.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Subscribes a diagnostic tracer to Autofac events.
        /// </summary>
        /// <param name="container">The container with the diagnostics to which you want to subscribe.</param>
        /// <param name="tracer">A diagnostic tracer that will be subscribed to the lifetime scope's diagnostic source.</param>
        /// <remarks>
        /// <para>
        /// This is a convenience method that attaches the <paramref name="tracer"/> to the
        /// <see cref="DiagnosticListener"/> associated with the <paramref name="container"/>. If you
        /// have an event listener that isn't a <see cref="DiagnosticTracerBase"/> you can
        /// use standard <see cref="DiagnosticListener"/> semantics to subscribe to the events
        /// with your custom listener.
        /// </para>
        /// </remarks>
        public static void SubscribeToDiagnostics(this IContainer container, DiagnosticTracerBase tracer)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (tracer is null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            container.DiagnosticSource.Subscribe(tracer, tracer.IsEnabled);
        }

        /// <summary>
        /// Subscribes a diagnostic tracer to Autofac events.
        /// </summary>
        /// <typeparam name="T">
        /// The type of diagnostic tracer that will be subscribed to the lifetime scope's diagnostic source.
        /// </typeparam>
        /// <param name="container">The container with the diagnostics to which you want to subscribe.</param>
        /// <returns>
        /// The diagnostic tracer that was created and attached to the diagnostic source. Use
        /// this instance to enable or disable the messages that should be handled.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method that attaches a tracer to the
        /// <see cref="DiagnosticListener"/> associated with the <paramref name="container"/>. If you
        /// have an event listener that isn't a <see cref="DiagnosticTracerBase"/> you can
        /// use standard <see cref="DiagnosticListener"/> semantics to subscribe to the events
        /// with your custom listener.
        /// </para>
        /// </remarks>
        public static T SubscribeToDiagnostics<T>(this IContainer container)
            where T : DiagnosticTracerBase, new()
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var tracer = new T();
            container.SubscribeToDiagnostics(tracer);
            return tracer;
        }
    }
}
