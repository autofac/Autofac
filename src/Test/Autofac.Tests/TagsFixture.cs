using System;
using Autofac.Builder;
using Autofac.Core.Lifetime;
using NUnit.Framework;
using Autofac.Core;
using System.Collections.Generic;

namespace Autofac.Tests
{
    [TestFixture]
    public class TagsFixture
    {
        class HomeController
        {
            public HomeController()
            {
            }
        }

        enum Tag { None, Outer, Middle, Inner }

        [Test]
        public void OuterSatisfiesInnerResolutions()
        {
            var builder = new ContainerBuilder();

            var instantiations = 0;

            builder.Register(c => { instantiations++; return ""; }).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);

            var root = builder.Build();

            var middle = root.BeginLifetimeScope(Tag.Middle);

            var inner = middle.BeginLifetimeScope(Tag.Inner);

            middle.Resolve<string>();
            root.Resolve<string>();
            inner.Resolve<string>();

            Assert.AreEqual(1, instantiations);
        }

        [Test]
        public void AnonymousInnerContainer()
        {
            var builder = new ContainerBuilder();

            var instantiations = 0;

            builder.Register(c => { instantiations++; return ""; })
                .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);

            var root = builder.Build();

            var anon = root.BeginLifetimeScope();

            anon.Resolve<string>();
            root.Resolve<string>();

            Assert.AreEqual(1, instantiations);
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void InnerRegistrationNotAccessibleToOuter()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => "")
                .InstancePerMatchingLifetimeScope(Tag.Middle);

            var outer = builder.Build();

            Assert.IsTrue(outer.IsRegistered<string>());
            outer.Resolve<string>();
        }

        [Test]
        public void TaggedRegistrationsAccessibleThroughNames()
        {
            var name = "Name";

            var builder = new ContainerBuilder();

            builder.Register(c => "")
                .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag)
                .Named<string>(name);

            var outer = builder.Build();

            var s = outer.Resolve<string>(name);
            Assert.IsNotNull(s);
        }

        [Test]
        public void CorrectScopeMaintainsOwnership()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new DisposeTracker())
                .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            var dt = inner.Resolve<DisposeTracker>();
            Assert.IsFalse(dt.IsDisposed);
            inner.Dispose();
            Assert.IsFalse(dt.IsDisposed);
            container.Dispose();
            Assert.IsTrue(dt.IsDisposed);
        }

        [Test]
        public void DefaultSingletonSemanticsCorrect()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            Assert.AreSame(container.Resolve<object>(), inner.Resolve<object>());
        }

        [Test]
        public void ReflectiveRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(object)).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            Assert.IsNotNull(container.Resolve<object>());
        }

        [Test]
        public void CollectionsAreTaggable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCollection<object>("o")
                .InstancePerMatchingLifetimeScope("tag")
                .As(typeof(IList<object>));

            var outer = builder.Build();
            var inner = outer.BeginLifetimeScope("tag");

            var coll = inner.Resolve<IList<object>>();
            Assert.IsNotNull(coll);

            var threw = false;
            try
            {
                outer.Resolve<IList<object>>();
            }
            catch (Exception)
            {
                threw = true;
            }

            Assert.IsTrue(threw);
        }

        [Test]
        public void GenericsAreTaggable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(List<>))
                .InstancePerMatchingLifetimeScope("tag")
                .As(typeof(IList<>));

            var outer = builder.Build();
            var inner = outer.BeginLifetimeScope("tag");

            var coll = inner.Resolve<IList<object>>();
            Assert.IsNotNull(coll);

            var threw = false;
            try
            {
                outer.Resolve<IList<object>>();
            }
            catch (Exception)
            {
                threw = true;
            }

            Assert.IsTrue(threw);
        }
    }
}
