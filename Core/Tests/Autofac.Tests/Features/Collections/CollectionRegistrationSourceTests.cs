using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Autofac.Builder;

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

            c.ComponentRegistry.Register(
                RegistrationBuilder.ForDelegate((ctx,p) => "World").CreateRegistration());

            Assert.AreEqual(2, c.Resolve<IEnumerable<string>>().Count());
        }

        public interface IFoo { }
        public class Foo1 : IFoo { }
        public class Foo2 : IFoo { }
        public class Foo3 : IFoo { }

        [Test]
        public void EnumerablesFromDifferentLifetimeScopesShouldReturnDifferentCollections()
        {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<Foo1>().As<IFoo>();
            var rootContainer = rootBuilder.Build();

            var scopeA = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo2>().As<IFoo>());
            var arrayA = scopeA.Resolve<IEnumerable<IFoo>>().ToArray();

            var scopeB = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo3>().As<IFoo>());
            var arrayB = scopeB.Resolve<IEnumerable<IFoo>>().ToArray();

            Assert.That(arrayA.Count(), Is.EqualTo(2));
            Assert.That(arrayA, Has.Some.TypeOf<Foo1>());
            Assert.That(arrayA, Has.Some.TypeOf<Foo2>());

            Assert.That(arrayB.Count(), Is.EqualTo(2));
            Assert.That(arrayB, Has.Some.TypeOf<Foo1>());
            Assert.That(arrayB, Has.Some.TypeOf<Foo3>());
        }
    }
}
