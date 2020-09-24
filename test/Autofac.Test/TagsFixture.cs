// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test
{
    public class TagsFixture
    {
        public enum Tag
        {
            None,
            Outer,
            Middle,
            Inner,
        }

        [Fact]
        public void AnonymousInnerContainer()
        {
            var builder = new ContainerBuilder();

            var instantiations = 0;

            builder.Register(c =>
            {
                instantiations++;
                return "";
            })
            .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);

            var root = builder.Build();

            var anon = root.BeginLifetimeScope();

            anon.Resolve<string>();
            root.Resolve<string>();

            Assert.Equal(1, instantiations);
        }

        [Fact]
        public void CollectionsAreTaggable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .InstancePerMatchingLifetimeScope("tag");

            var outer = builder.Build();
            var inner = outer.BeginLifetimeScope("tag");

            var coll = inner.Resolve<IList<object>>();
            Assert.Equal(1, coll.Count);

            Assert.Throws<DependencyResolutionException>(() => outer.Resolve<IList<object>>());
        }

        [Fact]
        public void CorrectScopeMaintainsOwnership()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new DisposeTracker())
                .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            var dt = inner.Resolve<DisposeTracker>();
            Assert.False(dt.IsDisposed);
            inner.Dispose();
            Assert.False(dt.IsDisposed);
            container.Dispose();
            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void DefaultSingletonSemanticsCorrect()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            Assert.Same(container.Resolve<object>(), inner.Resolve<object>());
        }

        [Fact]
        public void GenericsAreTaggable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(List<>))
                .InstancePerMatchingLifetimeScope("tag")
                .As(typeof(IList<>));

            var outer = builder.Build();
            var inner = outer.BeginLifetimeScope("tag");

            var coll = inner.Resolve<IList<object>>();
            Assert.NotNull(coll);

            Assert.Throws<DependencyResolutionException>(() => outer.Resolve<IList<object>>());
        }

        [Fact]
        public void InnerRegistrationNotAccessibleToOuter()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => "")
                .InstancePerMatchingLifetimeScope(Tag.Middle);

            var outer = builder.Build();

            Assert.True(outer.IsRegistered<string>());
            Assert.Throws<DependencyResolutionException>(() => outer.Resolve<string>());
        }

        [Fact]
        public void MatchesAgainstMultipleScopes()
        {
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";

            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InstancePerMatchingLifetimeScope(tag1, tag2);
            var container = builder.Build();

            var lifetimeScope = container.BeginLifetimeScope(tag1);
            Assert.NotNull(lifetimeScope.Resolve<object>());

            lifetimeScope = container.BeginLifetimeScope(tag2);
            Assert.NotNull(lifetimeScope.Resolve<object>());
        }

        [Fact]
        public void OuterSatisfiesInnerResolutions()
        {
            var builder = new ContainerBuilder();

            var instantiations = 0;

            builder.Register(c =>
            {
                instantiations++;
                return "";
            }).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);

            var root = builder.Build();

            var middle = root.BeginLifetimeScope(Tag.Middle);

            var inner = middle.BeginLifetimeScope(Tag.Inner);

            middle.Resolve<string>();
            root.Resolve<string>();
            inner.Resolve<string>();

            Assert.Equal(1, instantiations);
        }

        [Fact]
        public void ReflectiveRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(object)).InstancePerMatchingLifetimeScope(LifetimeScope.RootTag);
            var container = builder.Build();
            Assert.NotNull(container.Resolve<object>());
        }

        [Fact]
        public void TaggedRegistrationsAccessibleThroughNames()
        {
            var name = "Name";

            var builder = new ContainerBuilder();

            builder.Register(c => "")
                .InstancePerMatchingLifetimeScope(LifetimeScope.RootTag)
                .Named<string>(name);

            var outer = builder.Build();

            var s = outer.ResolveNamed<string>(name);
            Assert.NotNull(s);
        }

        public class HomeController
        {
        }
    }
}
