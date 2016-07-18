using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Autofac.Core.Resolving;
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
            Assert.True(de.Message.Contains("System.Object -> System.Object"));
            Assert.False(de.Message.Contains("System.Object -> System.Object -> System.Object"));
        }
    }
}
