using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void WcfRequestScoped_IsASynonymForMatchingRequestLifetime()
        {
            var cb = new ContainerBuilder();
            var regId = cb.RegisterType<object>().WcfRequestScoped().RegistrationStyle.Id;
            var c = cb.Build();
            var reg = (IComponentRegistration)c.ComponentRegistry.Registrations
                .Where(cr => cr.Id == regId)
                .Single();

            Assert.IsInstanceOf<MatchingScopeLifetime>(reg.Lifetime);
            Assert.AreEqual(InstanceSharing.Shared, reg.Sharing);
        }

        [Test]
        public void WcfRequestScoped_LifetimeScopeTagIsWcfRequestLifetime()
        {
            var cb = new ContainerBuilder();
            var regId = cb.Register(ctx => Guid.NewGuid()).As<Guid>().WcfRequestScoped();
            var c = cb.Build();
            Guid firstLifetimeValue;
            using (var l = c.BeginLifetimeScope(WcfLifetime.Request))
            {
                firstLifetimeValue = l.Resolve<Guid>();
                Assert.AreEqual(firstLifetimeValue, l.Resolve<Guid>());
            }
            Guid secondLifetimeValue;
            using (var l = c.BeginLifetimeScope(WcfLifetime.Request))
            {
                secondLifetimeValue = l.Resolve<Guid>();
                Assert.AreEqual(secondLifetimeValue, l.Resolve<Guid>());
            }
            Assert.AreNotEqual(firstLifetimeValue, secondLifetimeValue);
        }
    }
}
