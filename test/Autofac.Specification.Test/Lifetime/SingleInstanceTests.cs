// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class SingleInstanceTests
    {
        private interface IA
        {
        }

        [Fact]
        public void SingletonsCanBeRegisteredInNestedScope()
        {
            var rootScope = new ContainerBuilder().Build();
            var nestedScope = rootScope.BeginLifetimeScope(cb => cb.RegisterType<DisposeTracker>().SingleInstance());

            var dt = nestedScope.Resolve<DisposeTracker>();
            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.Same(dt, dt1);
        }

        [Fact]
        public void SingletonsRegisteredInContainerAreDisposedWithContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DisposeTracker>().SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<DisposeTracker>();
            var lifetime = c.BeginLifetimeScope();
            lifetime.Resolve<DisposeTracker>();
            lifetime.Dispose();
            Assert.False(a1.IsDisposed);
            c.Dispose();
            Assert.True(a1.IsDisposed);
        }

        [Fact]
        public void SingletonsRegisteredInNestedScopeAreDisposedWithThatScope()
        {
            var rootScope = new ContainerBuilder().Build();
            var nestedScope = rootScope.BeginLifetimeScope(cb => cb.RegisterType<DisposeTracker>().SingleInstance());
            var dt = nestedScope.Resolve<DisposeTracker>();
            nestedScope.Dispose();
            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void TypeAsSingleInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>()
                .As<IA>()
                .SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var a2 = c.Resolve<IA>();
            var a3 = c.BeginLifetimeScope().Resolve<IA>();
            var a4 = c.BeginLifetimeScope().Resolve<IA>();

            Assert.NotNull(a1);
            Assert.Same(a1, a2);
            Assert.Same(a1, a3);
            Assert.Same(a1, a4);
        }

        private class A : DisposeTracker, IA
        {
        }
    }
}
