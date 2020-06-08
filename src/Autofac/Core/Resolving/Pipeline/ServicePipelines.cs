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

using System.Collections.Generic;
using Autofac.Core.Resolving.Middleware;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Provides helpers for building service pipelines.
    /// </summary>
    public static class ServicePipelines
    {
        /// <summary>
        /// Gets the set of default middleware added to each service pipeline.
        /// </summary>
        public static IReadOnlyList<IResolveMiddleware> DefaultMiddleware { get; } = new IResolveMiddleware[]
        {
            CircularDependencyDetectorMiddleware.Default,
            ScopeSelectionMiddleware.Instance,
            SharingMiddleware.Instance,
            RegistrationPipelineInvokeMiddleware.Instance
        };

        /// <summary>
        /// Gets a default pre-built service pipeline that contains only the <see cref="DefaultMiddleware"/>.
        /// </summary>
        public static IResolvePipeline DefaultServicePipeline { get; } = new ResolvePipelineBuilder(PipelineType.Service)
                                                                            .UseRange(DefaultMiddleware)
                                                                            .Build();

        /// <summary>
        /// Checks whether a given resolve middleware is one of the default middleware in <see cref="DefaultMiddleware"/>.
        /// </summary>
        /// <param name="middleware">The middleware to test.</param>
        /// <returns>True if the middleware is one of the defaults; false otherwise.</returns>
        public static bool IsDefaultMiddleware(IResolveMiddleware middleware)
        {
            foreach (var defaultItem in DefaultMiddleware)
            {
                if (defaultItem == middleware)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
