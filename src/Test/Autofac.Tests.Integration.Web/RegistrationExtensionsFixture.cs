using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Component.Scope;
using Autofac.Component;
using Autofac.Integration.Web;

namespace Autofac.Tests.Integration.Web
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void HttpRequestScopedIsASynonymForContainerScoped()
        {
            var cb = new ContainerBuilder();
            var regId = cb.Register<object>().HttpRequestScoped().Id;
            var c = cb.Build();
            var reg = (Registration)c.ComponentRegistrations
                .Where(cr => cr.Descriptor.Id == regId)
                .Single();

            Assert.IsInstanceOfType(typeof(ContainerScope), reg.Scope);
        }
    }
}
