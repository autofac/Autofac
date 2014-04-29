using System.Net.Http;
using System.Web.Http.Hosting;
using Autofac.Core.Lifetime;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class CurrentRequestHandlerFixture
    {
        [Test]
        public void HandlerUpdatesDependencyScopeWithHttpRequestMessage()
        {
            var request = new HttpRequestMessage();
            var lifetimeScope = new ContainerBuilder().Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var scope = new AutofacWebApiDependencyScope(lifetimeScope);
            request.Properties.Add(HttpPropertyKeys.DependencyScope, scope);

            CurrentRequestHandler.UpdateScopeWithHttpRequestMessage(request);

            Assert.That(scope.GetService(typeof(HttpRequestMessage)), Is.EqualTo(request));
        }
    }
}
