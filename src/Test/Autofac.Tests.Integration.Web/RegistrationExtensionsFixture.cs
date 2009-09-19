using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Core.Lifetime;

namespace Autofac.Tests.Integration.Web
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void HttpRequestScopedIsASynonymForMatchingRequestLifetime()
        {
            var cb = new ContainerBuilder();
            var regId = cb.RegisterType<object>().HttpRequestScoped().RegistrationStyle.Id;
            var c = cb.Build();
            var reg = (IComponentRegistration)c.ComponentRegistry.Registrations
                .Where(cr => cr.Id == regId)
                .Single();

            Assert.IsInstanceOfType(typeof(MatchingScopeLifetime), reg.Lifetime);
            Assert.AreEqual(InstanceSharing.Shared, reg.Sharing);
        }
    }
}
