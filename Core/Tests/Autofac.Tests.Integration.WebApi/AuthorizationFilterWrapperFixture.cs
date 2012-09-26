using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class AuthorizationFilterWrapperFixture
    {
        [Test]
        public void RequiresFilterMetadata()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AuthorizationFilterWrapper(null));
            Assert.That(exception.ParamName, Is.EqualTo("filterMetadata"));
        }

        [Test]
        public void WrapperResolvesAuthorizationFilterFromDependencyScope()
        {
            var builder = ContainerBuilderFactory.Create();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacAuthorizationFilter>(c => new TestAuthorizationFilter(c.Resolve<ILogger>()))
                .AsAuthorizationFilterFor<TestController>(c => c.Get())
                .InstancePerApiRequest()
                .OnActivated(e => activationCount++);
            var container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration {DependencyResolver = resolver};
            var requestMessage = new HttpRequestMessage();
            requestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            var contollerContext = new HttpControllerContext {Request = requestMessage};
            var controllerDescriptor = new HttpControllerDescriptor {ControllerType = typeof(TestController)};
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            var actionContext = new HttpActionContext(contollerContext, actionDescriptor);
            var metadata = new Mock<IFilterMetadata>();
            metadata.Setup(mock => mock.ControllerType).Returns(typeof(TestController));
            metadata.Setup(mock => mock.FilterScope).Returns(FilterScope.Action);
            metadata.Setup(mock => mock.MethodInfo).Returns(methodInfo);

            var wrapper = new AuthorizationFilterWrapper(metadata.Object);

            wrapper.OnAuthorization(actionContext);
            Assert.That(activationCount, Is.EqualTo(1));
        }
    }
}