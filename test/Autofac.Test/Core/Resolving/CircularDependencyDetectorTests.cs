using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Autofac.Core.Resolving;
using Autofac.Core;

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
            Assert.Null(de.InnerException);
            Assert.True(de.Message.Contains("System.Object -> System.Object"));
            Assert.False(de.Message.Contains("System.Object -> System.Object -> System.Object"));
        }
    }
}
