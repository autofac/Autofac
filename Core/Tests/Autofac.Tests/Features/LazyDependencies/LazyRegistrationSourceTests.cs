#if !PORTABLE
using System;
using NUnit.Framework;

namespace Autofac.Tests.Features.LazyDependencies
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
            builder.RegisterType<object>();
            return builder.Build();
        }

        [Test]
        public void WhenTIsRegisteredByName_CanResolveLazyTByName()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named<object>("foo");
            var container = builder.Build();
            Assert.That(container.IsRegisteredWithName<Lazy<object>>("foo"));
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
#endif