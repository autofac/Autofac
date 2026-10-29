// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Test.Core.Resolving.Middleware;

public class SharingMiddlewareTests
{
    [Fact]
    public void SharedComponentWhosePipelineSetsNoInstanceThrows()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().SingleInstance();

        // Runs after sharing but skips the registration pipeline, so no instance is ever set.
        builder.RegisterServiceMiddleware<object>(PipelinePhase.ServicePipelineEnd, (context, next) => { });
        var container = builder.Build();

        var ex = Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());

        var inner = Assert.IsType<InvalidOperationException>(ex.GetBaseException());
        Assert.Equal("Instance null after pipeline invoke.", inner.Message);
    }
}
