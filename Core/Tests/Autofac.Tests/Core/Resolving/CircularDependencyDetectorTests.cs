using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Tests.Scenarios.Dependencies;
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
                builder.RegisterType<DependsByCtor>().SingleInstance();
                builder.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired();

                var target = builder.Build();
                target.Resolve<DependsByCtor>();
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsNull(de.InnerException);
                Assert.IsTrue(de.Message.Contains("Autofac.Tests.Scenarios.Dependencies.DependsByCtor -> Autofac.Tests.Scenarios.Dependencies.DependsByCtor"));
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
