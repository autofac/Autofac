using System;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.V1Compatibility.Builder
{
    [TestFixture]
    public class ProvidedInstanceRegistrationBuilderFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceNull()
        {
            var builder = new ContainerBuilder();
            builder.Register((object)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FactoryScopeNotValid()
        {
            var builder = new ContainerBuilder();
            builder.Register(new object()).WithScope(InstanceScope.Factory);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ContainerScopeNotValid()
        {
            var builder = new ContainerBuilder();
            builder.Register(new object()).WithScope(InstanceScope.Container);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DefaultScopeNotValid()
        {
            var builder = new ContainerBuilder();
            using (builder.SetDefaultScope(InstanceScope.Factory))
                builder.Register(new object());
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.Register("Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(string), cr.Descriptor.BestKnownImplementationType);
        }
    }
}
