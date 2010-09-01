using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Core.Lifetime;
using Autofac.Core;

namespace Autofac.Tests.Integration.Web
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void HttpRequestScopedIsASynonymForMatchingRequestLifetime()
        {
            var cb = new ContainerBuilder();
            var regId = cb.RegisterType<object>().InstancePerHttpRequest().RegistrationStyle.Id;
            var c = cb.Build();
            var reg = (IComponentRegistration)c.ComponentRegistry.Registrations
                .Where(cr => cr.Id == regId)
                .Single();

            Assert.IsInstanceOf<MatchingScopeLifetime>(reg.Lifetime);
            Assert.AreEqual(InstanceSharing.Shared, reg.Sharing);
        }
    }
}
