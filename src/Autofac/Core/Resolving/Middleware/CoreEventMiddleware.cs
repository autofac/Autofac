// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Middleware that invokes one of the built-in resolve events, defined by <see cref="ResolveEventType"/>.
    /// </summary>
    public class CoreEventMiddleware : IResolveMiddleware
    {
        private readonly Action<ResolveRequestContext, Action<ResolveRequestContext>> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEventMiddleware"/> class.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="phase">The phase of the pipeine at which the event runs.</param>
        /// <param name="callback">The middleware callback to process the event.</param>
        internal CoreEventMiddleware(ResolveEventType eventType, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            Phase = phase;
            _callback = callback;
            EventType = eventType;
        }

        /// <inheritdoc/>
        public PipelinePhase Phase { get; }

        /// <summary>
        /// Gets the event type represented by this middleware.
        /// </summary>
        public ResolveEventType EventType { get; }

        /// <inheritdoc/>
        public override string ToString() => EventType.ToString();

        /// <inheritdoc/>
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            _callback(context, next);
        }
    }
}
