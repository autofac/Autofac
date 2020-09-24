// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class InstancePerMatchingLifetimeScopeTests
    {
        [Fact]
        public void ChildOfNamedScopeGetsSharedInstance()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope(contextName);
            var child = ctx1.BeginLifetimeScope();

            Assert.Equal(ctx1.Resolve<object>(), child.Resolve<object>());
        }

        [Fact]
        public void InstancePerRequest_AdditionalLifetimeScopeTagsCanBeProvided()
        {
            var builder = new ContainerBuilder();
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";
            builder.Register(c => new object()).InstancePerRequest(tag1, tag2);

            var container = builder.Build();

            var scope1 = container.BeginLifetimeScope(tag1);
            Assert.NotNull(scope1.Resolve<object>());

            var scope2 = container.BeginLifetimeScope(tag2);
            Assert.NotNull(scope2.Resolve<object>());

            var requestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            Assert.NotNull(requestScope.Resolve<object>());
        }

        [Fact]
        public void InstancePerRequest_ResolutionSucceedsInRequestLifetime()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType(typeof(object)).InstancePerRequest();

            var container = builder.Build();
            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
            Assert.Throws<DependencyResolutionException>(() => container.BeginLifetimeScope().Resolve<object>());

            var apiRequestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            Assert.NotNull(apiRequestScope.Resolve<object>());
        }

        [Fact]
        public void MustHaveMatchingScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope("ctx");
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope();
            Assert.Throws<DependencyResolutionException>(() => ctx1.Resolve<object>());
        }

        [Fact]
        public void SharingWithinNamedScope()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope(contextName);
            var ctx2 = container.BeginLifetimeScope(contextName);

            Assert.Equal(ctx1.Resolve<object>(), ctx1.Resolve<object>());
            Assert.NotEqual(ctx1.Resolve<object>(), ctx2.Resolve<object>());
        }
    }
}
