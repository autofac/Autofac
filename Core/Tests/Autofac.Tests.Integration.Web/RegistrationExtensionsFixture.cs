using System.Linq;
using NUnit.Framework;
using Autofac.Integration.Web;
using Autofac.Core.Lifetime;
using Autofac.Core;

namespace Autofac.Tests.Integration.Web
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void AdditionalLifetimeScopeTagsCanBeProvidedToInstancePerHttpRequest()
        {
            var builder = new ContainerBuilder();
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";
            builder.Register(c => new object()).InstancePerHttpRequest(tag1, tag2);

            var container = builder.Build();

            var scope1 = container.BeginLifetimeScope(tag1);
            Assert.That(scope1.Resolve<object>(), Is.Not.Null);

            var scope2 = container.BeginLifetimeScope(tag2);
            Assert.That(scope2.Resolve<object>(), Is.Not.Null);

            var requestScope = container.BeginLifetimeScope(WebLifetime.Request);
            Assert.That(requestScope.Resolve<object>(), Is.Not.Null);
        }

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
