using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Modules;

namespace Autofac.Tests.V1Compatibility.Modules
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
            cb.Register(s1);
            cb.Register(s2);
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
            cb.Register(s1);
            cb.Register(s2);
            var c = cb.Build();

            var strings = c.Resolve<IEnumerable<string>>();
            Assert.AreEqual(2, strings.Count());
            Assert.IsTrue(strings.Contains(s1));
            Assert.IsTrue(strings.Contains(s2));
        }

        [Test]
        public void ResolvesAllAvailableElementsInHierarchy()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule(new ImplicitCollectionSupportModule());
            var s1 = "Hello";
            cb.Register(s1);
            var c = cb.Build();
            var inner = c.CreateInnerContainer();

            var cb2 = new ContainerBuilder();
            var s2 = "World";
            cb2.Register(s2);
            cb2.Build(inner);

            var strings = inner.Resolve<IEnumerable<string>>();
            Assert.AreEqual(2, strings.Count());
            Assert.IsTrue(strings.Contains(s1));
            Assert.IsTrue(strings.Contains(s2));
        }
    }
}
