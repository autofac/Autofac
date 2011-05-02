using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void RegisterModelBinderProviderThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinderProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("builder"));
        }

        [Test]
        public void RegisterModelBindersThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinders(null, Assembly.GetExecutingAssembly()));
            Assert.That(exception.ParamName, Is.EqualTo("builder"));
        }

        [Test]
        public void RegisterModelBindersThrowsExceptionForNullAssemblies()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ContainerBuilder().RegisterModelBinders(null));
            Assert.That(exception.ParamName, Is.EqualTo("modelBinderAssemblies"));
        }

        [Test]
        public void RegisterFilterProviderThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.RegisterFilterProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("builder"));
        }

        [Test]
        public void RegisterFilterProviderRemovesExistingProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterFilterProvider();
            Assert.That(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().Any(), Is.False);
        }

        [Test]
        public void RegisterFilterProviderCanSafelyBeCalledTwice()
        {
            var builder = new ContainerBuilder();
            builder.RegisterFilterProvider();
            builder.RegisterFilterProvider();
        }

        [Test]
        public void CacheInSessionThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.CacheInSession<object, SimpleActivatorData, SingleRegistrationStyle>(null));
            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void InvokesCustomActivating()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(GetType().Assembly)
                .OnActivating(e => ((ModuleTestController)e.Instance).Dependency = new object());

            var container = builder.Build();

            var controller = container.Resolve<ModuleTestController>();
            Assert.IsNotNull(controller.Dependency);
        }

        [Test]
        public void InjectsInvoker()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(GetType().Assembly)
                .InjectActionInvoker();
            builder.RegisterType<TestActionInvoker>().As<IActionInvoker>();
            var container = builder.Build();

            var controller = container.Resolve<ModuleTestController>();
            Assert.IsInstanceOf<TestActionInvoker>(controller.ActionInvoker);
        }


        [Test]
        public void DoesNotRegisterControllerTypesThatDoNotEndWithControllerString()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(GetType().Assembly);

            var container = builder.Build();

            Assert.IsFalse(container.IsRegistered<IsAControllerNot>());
        }

        public class TestActionInvoker : IActionInvoker
        {
            public bool InvokeAction(ControllerContext controllerContext, string actionName)
            {
                return true;
            }
        }

        public class ModuleTestController : Controller
        {
            public object Dependency;
        }

        public class IsAControllerNot : Controller
        {
        }
    }
}