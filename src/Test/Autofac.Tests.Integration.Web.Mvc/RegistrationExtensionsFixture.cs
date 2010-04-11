using System;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        readonly IControllerIdentificationStrategy _defaultIdentificationStrategy =
            new DefaultControllerIdentificationStrategy();

        [Test]
        public void InvokesCustomActivating()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(_defaultIdentificationStrategy, GetType().Assembly)
                .OnActivating(e => ((ModuleTestController)e.Instance).Dependency = new object());

            var container = builder.Build();

            var controller = (ModuleTestController)container.Resolve(_defaultIdentificationStrategy.ServiceForControllerType(typeof(ModuleTestController)));
            Assert.IsNotNull(controller.Dependency);
        }

        [Test]
        public void InjectsInvoker()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(_defaultIdentificationStrategy, GetType().Assembly)
                .InjectActionInvoker();
            builder.RegisterType<TestActionInvoker>().As<IActionInvoker>();
            var container = builder.Build();

            var controller = (ModuleTestController)container.Resolve(_defaultIdentificationStrategy.ServiceForControllerType(typeof(ModuleTestController)));
            Assert.IsInstanceOf<TestActionInvoker>(controller.ActionInvoker);
        }


        [Test]
        public void DoesNotRegisterControllerTypesThatDoNotEndWithControllerString()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(_defaultIdentificationStrategy, GetType().Assembly);

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