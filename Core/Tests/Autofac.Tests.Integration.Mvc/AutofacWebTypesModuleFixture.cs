using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacWebTypesModuleFixture
    {
        [Test]
        [TestCase(typeof(HttpContextBase))]
        [TestCase(typeof(HttpRequestBase))]
        [TestCase(typeof(HttpResponseBase))]
        [TestCase(typeof(HttpServerUtilityBase))]
        [TestCase(typeof(HttpSessionStateBase))]
        [TestCase(typeof(HttpApplicationStateBase))]
        [TestCase(typeof(HttpBrowserCapabilitiesBase))]
        [TestCase(typeof(HttpFileCollectionBase))]
        [TestCase(typeof(RequestContext))]
        [TestCase(typeof(HttpCachePolicyBase))]
        [TestCase(typeof(VirtualPathProvider))]
        [TestCase(typeof(UrlHelper))]
        public void EnsureWebTypeIsRegistered(Type serviceType)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacWebTypesModule());
            IContainer container = builder.Build();
            Assert.That(container.IsRegistered(serviceType), Is.True);
        }
    }
}
