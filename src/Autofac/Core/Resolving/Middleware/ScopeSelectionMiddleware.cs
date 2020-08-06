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
using System.Globalization;
using System.Text;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Selects the correct activation scope based on the registration's lifetime.
    /// </summary>
    internal class ScopeSelectionMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ScopeSelectionMiddleware"/>.
        /// </summary>
        public static ScopeSelectionMiddleware Instance => new ScopeSelectionMiddleware();

        private ScopeSelectionMiddleware()
        {
            // Only want to use the static instance.
        }

        /// <inheritdoc/>
        public PipelinePhase Phase => PipelinePhase.ScopeSelection;

        /// <inheritdoc/>
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            try
            {
                context.ChangeScope(context.Registration.Lifetime.FindScope(context.ActivationScope));
            }
            catch (DependencyResolutionException ex)
            {
                var services = new StringBuilder();
                foreach (var s in context.Registration.Services)
                {
                    services.Append("- ");
                    services.AppendLine(s.Description);
                }

                var message = string.Format(CultureInfo.CurrentCulture, MiddlewareMessages.UnableToLocateLifetimeScope, context.Registration.Activator.LimitType, services);
                throw new DependencyResolutionException(message, ex);
            }

            next(context);
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(ScopeSelectionMiddleware);
    }
}
