using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;
using Autofac.Tests.Scenarios.Dependencies;

namespace Autofac.Tests.Core.Resolving
{
    [TestFixture]
    public class ResolveOperationTests
    {
        [Test]
        public void OnCircularDependency_MessageDescribesCycle()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterDelegate(c => c.Resolve<object>());

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

        [Test]
        public void CtorPropDependencyOkOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(true);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();

            Assert.IsNotNull(dbp.Dep);
            Assert.IsNotNull(dbp.Dep.Dep);
            Assert.AreSame(dbp, dbp.Dep.Dep);
        }

        [Test]
        public void CtorPropDependencyOkOrder2()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(true);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.IsNotNull(dbc.Dep);
            Assert.IsNotNull(dbc.Dep.Dep);
            Assert.AreSame(dbc, dbc.Dep.Dep);
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void CtorPropDependencyFactoriesOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>();
            cb.RegisterType<DependsByProp>().PropertiesAutowired(true);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void CtorPropDependencyFactoriesOrder2()
        {
            var cb = new ContainerBuilder();
            var ac = 0;
            cb.RegisterType<DependsByCtor>().OnActivating(e => { ++ac; });
            cb.RegisterType<DependsByProp>().OnActivating(e => { ++ac; })
                .PropertiesAutowired(true);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.AreEqual(2, ac);
        }
    }
}
