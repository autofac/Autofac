using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Autofac.Integration.Owin;
using Autofac.Integration.WebApi;
using Autofac.Integration.WebApi.Owin;
using Microsoft.Owin;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi.Owin
{
    [TestFixture]
    public class DependencyScopeHandlerFixture
    {
        [Test]
        public void InvokeMethodThrowsExceptionIfRequestNull()
        {
            var handler = new DependencyScopeHandler();
            var invoker = new HttpMessageInvoker(handler);

            var exception = Assert.Throws<ArgumentNullException>(() => invoker.SendAsync(null, new CancellationToken()));

            Assert.That(exception.ParamName, Is.EqualTo("request"));
        }

        [Test]
        public async void AddsAutofacDependencyScopeToHttpRequestMessage()
        {
            var request = new HttpRequestMessage();
            var context = new OwinContext();
            request.Properties.Add("MS_OwinContext", context);

            var container = new ContainerBuilder().Build();
            context.Set(Constants.OwinLifetimeScopeKey, container);

            var fakeHandler = new FakeInnerHandler {Message = new HttpResponseMessage(HttpStatusCode.OK)};
            var handler = new DependencyScopeHandler {InnerHandler = fakeHandler};
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(request, new CancellationToken());

            var scope = (AutofacWebApiDependencyScope)request.Properties[HttpPropertyKeys.DependencyScope];

            Assert.That(scope.LifetimeScope, Is.EqualTo(container));
        }
    }

    public class FakeInnerHandler : DelegatingHandler
    {
        public HttpResponseMessage Message { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Message == null ? base.SendAsync(request, cancellationToken) : Task.Run(() => Message, cancellationToken);
        }
    }
}