// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Middleware that starts startable components.
    /// </summary>
    internal class StartableMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="StartableMiddleware"/>.
        /// </summary>
        public static StartableMiddleware Instance { get; } = new StartableMiddleware();

        private StartableMiddleware()
        {
        }

        /// <inheritdoc/>
        public PipelinePhase Phase => PipelinePhase.Activation;

        /// <inheritdoc />
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            next(context);

            if (context.Instance is IStartable startable
                && !context.Registration.Metadata.ContainsKey(MetadataKeys.AutoActivated)
                && context.ComponentRegistry.Properties.ContainsKey(MetadataKeys.StartOnActivatePropertyKey))
            {
                // Issue #916: Set the startable as "done starting" BEFORE calling Start
                // so you don't get a StackOverflow if the component creates a child scope
                // during Start. You don't want the startable trying to activate itself.
                context.Registration.Metadata[MetadataKeys.AutoActivated] = true;
                startable.Start();
            }
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(StartableMiddleware);
    }
}
