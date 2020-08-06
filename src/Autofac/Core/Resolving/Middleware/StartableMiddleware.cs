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
