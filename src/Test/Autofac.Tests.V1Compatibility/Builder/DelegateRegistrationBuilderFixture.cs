using System;
using Autofac.Builder;
using Autofac.Registrars.Delegate;
using NUnit.Framework;

namespace Autofac.Tests.V1Compatibility.Builder
{
    [TestFixture]
    public class DelegateRegistrationBuilderFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDelegateNull()
        {
            var target = new ContainerBuilder();
            target.Register((ComponentActivator<object>)null);
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.Register(c => "Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(string), cr.Descriptor.BestKnownImplementationType);
        }
    }
}
