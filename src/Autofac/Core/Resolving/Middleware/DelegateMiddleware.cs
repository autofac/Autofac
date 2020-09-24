// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Wraps pipeline delegates from the Use* methods in <see cref="PipelineBuilderExtensions" />.
    /// </summary>
    internal class DelegateMiddleware : IResolveMiddleware
    {
        private readonly string _descriptor;
        private readonly Action<ResolveRequestContext, Action<ResolveRequestContext>> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateMiddleware"/> class.
        /// </summary>
        /// <param name="descriptor">The middleware description.</param>
        /// <param name="phase">The pipeline phase.</param>
        /// <param name="callback">The callback to execute.</param>
        public DelegateMiddleware(string descriptor, PipelinePhase phase, Action<ResolveRequestContext, Action<ResolveRequestContext>> callback)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Phase = phase;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc />
        public PipelinePhase Phase { get; }

        /// <inheritdoc />
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            _callback(context, next);
        }

        /// <inheritdoc />
        public override string ToString() => _descriptor;
    }
}
