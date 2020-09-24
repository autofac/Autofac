// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class CircularDependencyDetectorTests
    {
        [Fact]
        public void OnCircularDependency_MessageDescribesCycle()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => c.Resolve<object>());

            var target = builder.Build();
            var de = Assert.Throws<DependencyResolutionException>(() => target.Resolve<object>());
            Assert.Contains("λ:System.Object -> λ:System.Object", de.ToString());
            Assert.DoesNotContain("λ:System.Object -> λ:System.Object -> λ:System.Object", de.Message);
        }
    }
}
