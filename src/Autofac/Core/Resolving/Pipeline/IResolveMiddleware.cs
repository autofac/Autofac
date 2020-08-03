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

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines an executable resolve middleware.
    /// </summary>
    public interface IResolveMiddleware
    {
        /// <summary>
        /// Gets the phase of the resolve pipeline at which to execute.
        /// </summary>
        PipelinePhase Phase { get; }

        /// <summary>
        /// Invoked when this middleware is executed as part of an active <see cref="ResolveRequest"/>. The middleware should usually call
        /// the <paramref name="next"/> method in order to continue the pipeline, unless the middleware fully satisfies the request.
        /// </summary>
        /// <param name="context">The context for the resolve request.</param>
        /// <param name="next">The method to invoke to continue the pipeline execution; pass this method the <paramref name="context"/> argument.</param>
        void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next);
    }
}
