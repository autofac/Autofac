// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
