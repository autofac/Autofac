using Autofac.Core.Resolving.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Core.Resolving.Pipeline
{
    public static class ServicePipelines
    {
        public static readonly IResolveMiddleware[] DefaultStages = new IResolveMiddleware[]
        {
            CircularDependencyDetectorMiddleware.Default,
            ScopeSelectionMiddleware.Instance,
            SharingMiddleware.Instance,
            DefaultServicePipelineTerminatorMiddleware.Instance
        };

        public static IResolvePipeline DefaultServicePipeline { get; } = new ResolvePipelineBuilder(PipelineType.Service)
                                                                            .UseRange(DefaultStages)
                                                                            .Build();

        public static bool IsDefaultMiddleware(IResolveMiddleware middleware)
        {
            foreach (var defaultItem in DefaultStages)
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
