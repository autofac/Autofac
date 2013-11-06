using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacFilterProviderFixture
    {
        ControllerContext _baseControllerContext;
        ControllerDescriptor _controllerDescriptor;

        MethodInfo _baseMethodInfo;
        string _actionName;

        ReflectedActionDescriptor _reflectedActionDescriptor;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _baseControllerContext = new ControllerContext {Controller = new TestController()};

            _baseMethodInfo = TestController.GetAction1MethodInfo<TestController>();
            _actionName = _baseMethodInfo.Name;

            _controllerDescriptor = new Mock<ControllerDescriptor>().Object;
            _reflectedActionDescriptor = new ReflectedActionDescriptor(_baseMethodInfo, _actionName, _controllerDescriptor);
        }

        [Test]
        public void FilterRegistrationsWithoutMetadataIgnored()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AuthorizeAttribute>().AsImplementedInterfaces();
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_baseControllerContext, _reflectedActionDescriptor).ToList();
            Assert.That(filters, Has.Count.EqualTo(0));
        }

        [Test]
        public void CanRegisterMultipleFilterTypesAgainstSingleService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new TestCombinationFilter())
                .AsActionFilterFor<TestController>()
                .AsAuthenticationFilterFor<TestController>()
                .AsAuthorizationFilterFor<TestController>()
                .AsExceptionFilterFor<TestController>()
                .AsResultFilterFor<TestController>();
            var container = builder.Build();

            Assert.That(container.Resolve<IActionFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IAuthenticationFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IAuthorizationFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IExceptionFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IResultFilter>(), Is.Not.Null);
        }

        static void SetupMockLifetimeScopeProvider(ILifetimeScope container)
        {
            var resolver = new AutofacDependencyResolver(container, new StubLifetimeScopeProvider(container));
            DependencyResolver.SetResolver(resolver);
        }
    }
}
