using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class AutofacControllerFactoryFixture
    {
        readonly string _apiRequestTag = AutofacControllerFactory.ApiRequestTag;

        [Test]
        public void NullContainerThrowsException()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacControllerFactory(configuration, null));

            Assert.That(exception.ParamName, Is.EqualTo("container"));
        }

        [Test]
        public void NullConfigurationThrowsException()
        {
            var container = new ContainerBuilder().Build();

            Assert.Throws<ArgumentNullException>(
                () => new AutofacControllerFactory(null, container));
        }

        [Test]
        public void CreateControllerBeginsNewLifetimeScope()
        {
            var container = new Mock<ILifetimeScope>();
            var lifetimeScope = new Mock<ILifetimeScope>();
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var factory = new AutofacControllerFactory(configuration, container.Object);

            container.Setup(mock => mock.BeginLifetimeScope(_apiRequestTag)).Returns(lifetimeScope.Object).Verifiable();

            factory.CreateController(controllerContext, "Test");

            container.VerifyAll();
        }

        [Test]
        public void CreateControllerAddsLifetimeScopeToRequestMessageProperties()
        {
            var container = new Mock<ILifetimeScope>();
            var lifetimeScope = new Mock<ILifetimeScope>();
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var factory = new AutofacControllerFactory(configuration, container.Object);

            container.Setup(mock => mock.BeginLifetimeScope(_apiRequestTag)).Returns(lifetimeScope.Object);

            factory.CreateController(controllerContext, "Test");

            Assert.That(requestMessage.Properties.ContainsKey(_apiRequestTag), Is.True);
        }

        [Test]
        public void ReleaseControllerDisposesLifetimeScope()
        {
            var container = new Mock<ILifetimeScope>();
            var lifetimeScope = new Mock<ILifetimeScope>();
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var factory = new AutofacControllerFactory(configuration, container.Object);

            container.Setup(mock => mock.BeginLifetimeScope(_apiRequestTag)).Returns(lifetimeScope.Object);
            lifetimeScope.Setup(mock => mock.Dispose()).Verifiable();

            var controller = factory.CreateController(controllerContext, "Test");

            factory.ReleaseController(controller);

            lifetimeScope.VerifyAll();
        }

        [Test]
        public void ActivatorReturningNullControllerDisposesLifetimeScope()
        {
            var container = new Mock<ILifetimeScope>();
            var lifetimeScope = new Mock<ILifetimeScope>();
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var activator = new Mock<IHttpControllerActivator>();
            var factory = new AutofacControllerFactory(configuration, container.Object);

            configuration.ServiceResolver.SetService(typeof(IHttpControllerActivator), activator.Object);

            activator.Setup(mock => mock.Create(controllerContext, typeof(TestController))).Returns((IHttpController) null);
            container.Setup(mock => mock.BeginLifetimeScope(_apiRequestTag)).Returns(lifetimeScope.Object);
            lifetimeScope.Setup(mock => mock.Dispose()).Verifiable();

            Assert.Throws<ArgumentNullException>(() => factory.CreateController(controllerContext, "Test"));

            lifetimeScope.VerifyAll();
        }
    }
}