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
            builder.RegisterInstance(new object()).NoInstanceSharing();
        }

        [Test]
        [Ignore("Not implemented.")]
        [ExpectedException(typeof(ArgumentException))]
        public void ContainerScopeNotValid()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).ShareInstanceInLifetimeScope();
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(string), cr.Activator.LimitType);
        }
    }
}
