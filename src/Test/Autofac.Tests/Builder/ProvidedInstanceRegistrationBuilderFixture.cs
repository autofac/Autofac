using System;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ProvidedInstanceRegistrationBuilderFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceNull()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance((object)null);
        }

        [Test]
        [Ignore("Not implemented.")]
        [ExpectedException(typeof(ArgumentException))]
        public void FactoryScopeNotValid()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
        }

        [Test]
        [Ignore("Not implemented.")]
        [ExpectedException(typeof(ArgumentException))]
        public void ContainerScopeNotValid()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerLifetimeScope();
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
