// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;
using Moq;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class CircularDependencyMiddlewareTests
    {
        [Fact]
        public void NextCalled_OperationIsNotIDependencyTrackingResolveOperation_MiddlewareSkipped()
        {
            var resolveRequestContextMock = new Mock<ResolveRequestContext>();
            var middleware =
                new CircularDependencyDetectorMiddleware(CircularDependencyDetectorMiddleware.DefaultMaxResolveDepth);

            middleware.Execute(resolveRequestContextMock.Object, context => { });

            resolveRequestContextMock.Verify(context => context.Operation.RequestDepth, Times.Never);
        }
    }
}
