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
