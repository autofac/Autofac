using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core.Resolving;
using Autofac.Core;

namespace Autofac.Tests.Core.Resolving
{
    [TestFixture]
    public class CircularDependencyDetectorTests
    {
        [Test]
        public void OnCircularDependency_MessageDescribesCycle()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.Register(c => c.Resolve<object>());

                var target = builder.Build();
                target.Resolve<object>();
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsNull(de.InnerException);
                Assert.IsTrue(de.Message.Contains("System.Object -> System.Object"));
                Assert.IsFalse(de.Message.Contains("System.Object -> System.Object -> System.Object"));
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected a DependencyResolutionException, got {0}.", ex);
                return;
            }

            Assert.Fail("Expected a DependencyResolutionException.");
        }
    }
}
