// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class InstanceRegistrationTests
    {
        private interface IA
        {
        }

        private interface IB
        {
        }

        [Fact]
        public void NullCannotBeRegisteredAsAnInstance()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterInstance((object)null));
        }

        [Fact]
        public void ProvidedInstancesCannotSupportInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void ProvidedInstancesCannotSupportInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerLifetimeScope();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void RegisterInstanceAsImplementedInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new A()).AsImplementedInterfaces();
            var context = builder.Build();
            context.Resolve<IA>();
            context.Resolve<IB>();
        }

        [Fact]
        public void RegisterInstanceAsSelf()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new A()).AsSelf();
            var context = builder.Build();

            context.Resolve<A>();
        }

        [Fact]
        public void RegisterInstanceAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        private class A : IA, IB
        {
        }
    }
}
