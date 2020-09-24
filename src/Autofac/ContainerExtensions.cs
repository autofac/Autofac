// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
