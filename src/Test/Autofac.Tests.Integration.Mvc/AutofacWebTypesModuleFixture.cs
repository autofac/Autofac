using System.Web;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacWebTypesModuleFixture
    {
        [Test]
        public void EnsureHttpContextRelatedTypesRegistered()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacWebTypesModule());
            IContainer container = builder.Build();

            Assert.That(container.IsRegistered<HttpContextBase>(), Is.True);
            Assert.That(container.IsRegistered<HttpRequestBase>(), Is.True);
            Assert.That(container.IsRegistered<HttpResponseBase>(), Is.True);
            Assert.That(container.IsRegistered<HttpServerUtilityBase>(), Is.True);
            Assert.That(container.IsRegistered<HttpSessionStateBase>(), Is.True);
        }
    }
}
