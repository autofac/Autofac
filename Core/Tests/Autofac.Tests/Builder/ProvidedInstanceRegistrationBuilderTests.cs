using System;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ProvidedInstanceRegistrationBuilderTests
    {
        [Test]
        public void NullCannotBeRegisteredAsAnInstance()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterInstance((object)null));
        }

        [Test]
        public void InstancePerDependency_NotValidForProvidedInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void InstancePerLifetimeScope_NotValidForProvidedInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerLifetimeScope();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void LimitType_ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(string), cr.Activator.LimitType);
        }

        [Test]
        public void DefaultServiceType_IsStaticTypeOfRegisteredInstance()
        {
            object instance = "Hello";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance);
            var container = builder.Build();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<string>();
        }
    }
}
