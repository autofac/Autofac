using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Features.Collections;

namespace Autofac.Tests.Features.Collections
{
    [TestFixture]
    public class CollectionRegistrationSourceTests
    {
        [Test]
        public void ResolvesAllAvailableElementsWhenArrayIsRequested()
        {
            var cb = new ContainerBuilder();
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

        [Test]
        public void ResolvesCollectionItemsFromCurrentLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCollection<DisposeTracker>("dt");
            builder.RegisterType<DisposeTracker>().MemberOf("dt");
            var container = builder.Build();

            DisposeTracker tracker;
            using (var ls = container.BeginLifetimeScope())
                tracker = ls.Resolve<DisposeTracker[]>().First();

            Assert.IsTrue(tracker.IsDisposed);
        }

        [Test]
        public void ReflectsChangesInComponentRegistry()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello");
            var c = cb.Build();
            Assert.AreEqual(1, c.Resolve<IEnumerable<string>>().Count());
            c.Configure(cb2 => cb2.RegisterInstance("World"));

            Assert.AreEqual(2, c.Resolve<IEnumerable<string>>().Count());
        }
    }
}
