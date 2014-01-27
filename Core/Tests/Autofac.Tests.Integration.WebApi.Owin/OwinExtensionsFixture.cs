using System.Linq;
using System.Web.Http;
using Autofac.Integration.WebApi.Owin;
using Microsoft.Owin.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi.Owin
{
    [TestFixture]
    public class OwinExtensionsFixture
    {
        [Test]
        public void UseAutofacWebApiAddsDelegatingHandler()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration, new ContainerBuilder().Build());

            Assert.That(configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void UseAutofacWebApiWillOnlyAddDelegatingHandlerOnce()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();
            var container = new ContainerBuilder().Build();

            app.UseAutofacWebApi(configuration, container);
            app.UseAutofacWebApi(configuration, container);

            Assert.That(configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count(), Is.EqualTo(1));
        }
    }
}