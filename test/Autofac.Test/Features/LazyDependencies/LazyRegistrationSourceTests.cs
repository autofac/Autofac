// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

namespace Autofac.Test.Features.LazyDependencies
{
    public class LazyRegistrationSourceTests
    {
        [Fact]
        public void WhenTIsRegistered_CanResolveLazyT()
        {
            var container = GetContainerWithLazyObject();
            Assert.True(container.IsRegistered<Lazy<object>>());
        }

        private static IContainer GetContainerWithLazyObject()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            return builder.Build();
        }

        [Fact]
        public void WhenTIsRegisteredByName_CanResolveLazyTByName()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named<object>("foo");
            var container = builder.Build();
            Assert.True(container.IsRegisteredWithName<Lazy<object>>("foo"));
        }

        [Fact]
        public void WhenLazyIsResolved_ValueProvided()
        {
            var container = GetContainerWithLazyObject();
            var lazy = container.Resolve<Lazy<object>>();
            Assert.IsType<object>(lazy.Value);
        }

        [Fact]
        public void WhenLazyIsResolved_ValueIsNotYetCreated()
        {
            var container = GetContainerWithLazyObject();
            var lazy = container.Resolve<Lazy<object>>();
            Assert.False(lazy.IsValueCreated);
        }

        [Fact]
        public void LazyWorksWithCircularPropertyDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A>()
                .SingleInstance();
            builder.RegisterType<B>()
                .SingleInstance()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var container = builder.Build();
            Assert.NotNull(container.Resolve<A>());
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        private class A
        {
            public A(Lazy<B> b)
            {
            }
        }

        private class B
        {
            public A A { get; set; }
        }

#pragma warning restore IDE0060

    }
}
