using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Modules;

namespace Autofac.Tests.Modules
{
    [TestFixture]
    public class ImplicitCollectionSupportModuleFixture
    {
        [Test]
        public void ResolvesAllAvailableElementsWhenArrayIsRequested()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new ImplicitCollectionSupportModule());
            var s1 = "Hello";
            var s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<string[]>();
            Assert.AreEqual(2, strings.Length);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
        }

        [Test]
        public void ResolvesAllAvailableElementsWhenEnumerableIsRequested()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new ImplicitCollectionSupportModule());
            var s1 = "Hello";
            var s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<IEnumerable<string>>();
            Assert.AreEqual(2, strings.Count());
            Assert.IsTrue(strings.Contains(s1));
            Assert.IsTrue(strings.Contains(s2));
        }

        //[Test]
        //public void ResolvesAllAvailableElementsInHierarchy()
        //{
        //    var cb = new ContainerBuilder();
        //    cb.RegisterModule(new ImplicitCollectionSupportModule());
        //    var s1 = "Hello";
        //    cb.RegisterInstance(s1);
        //    var c = cb.Build();
        //    var inner = c.BeginLifetimeScope();

        //    var cb2 = new ContainerBuilder();
        //    var s2 = "World";
        //    cb2.RegisterInstance(s2);
        //    cb2.Build(inner);

        //    var strings = inner.Resolve<IEnumerable<string>>();
        //    Assert.AreEqual(2, strings.Count());
        //    Assert.IsTrue(strings.Contains(s1));
        //    Assert.IsTrue(strings.Contains(s2));
        //}
    }
}
