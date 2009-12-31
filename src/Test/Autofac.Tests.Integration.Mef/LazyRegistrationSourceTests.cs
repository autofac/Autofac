using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class LazyRegistrationSourceTests
    {
        [Test]
        public void WhenTIsRegistered_CanResolveLazyT()
        {
            var container = GetContainerWithLazyObject();
            Assert.That(container.IsRegistered<Lazy<object>>());
        }

        static IContainer GetContainerWithLazyObject()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new LazyRegistrationSource());
            builder.RegisterType<object>();
            return builder.Build();
        }

        [Test]
        public void WhenTIsRegisteredByName_CanResolveLazyTByName()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new LazyRegistrationSource());
            builder.RegisterType<object>().Named<object>("foo");
            var container = builder.Build();
            Assert.That(container.IsRegistered<Lazy<object>>("foo"));
        }

        [Test]
        public void WhenLazyIsResolved_ValueProvided()
        {
            var container = GetContainerWithLazyObject();
            var lazy = container.Resolve<Lazy<object>>();
            Assert.IsInstanceOf<object>(lazy.Value);
        }

        [Test]
        public void WhenLazyIsResolved_ValueIsNotYetCreated()
        {
            var container = GetContainerWithLazyObject();
            var lazy = container.Resolve<Lazy<object>>();
            Assert.IsFalse(lazy.IsValueCreated);
        }
    }
}
