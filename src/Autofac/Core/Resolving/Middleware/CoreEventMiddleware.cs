using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Middleware that invokes one of the built-in resolve events, defined by <see cref="ResolveEventType"/>.
    /// </summary>
    public class CoreEventMiddleware : IResolveMiddleware
    {
        private readonly Action<IResolveRequestContext, Action<IResolveRequestContext>> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEventMiddleware"/> class.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="phase">The phase of the pipeine at which the event runs.</param>
        /// <param name="callback">The middleware callback to process the event.</param>
        internal CoreEventMiddleware(ResolveEventType eventType, PipelinePhase phase, Action<IResolveRequestContext, Action<IResolveRequestContext>> callback)
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
        public void Execute(IResolveRequestContext context, Action<IResolveRequestContext> next)
        {
            _callback(context, next);
        }
    }
}
