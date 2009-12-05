using System;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web.Mvc
{
	[TestFixture]
	public class AutofacControllerModuleFixture
	{
		[Test]
		public void InvokesCustomActivating()
		{
			var builder = new ContainerBuilder();
            var module = new AutofacControllerModule(GetType().Assembly)
            {
                ActivatingHandler = ((sender, args) => ((ModuleTestController)args.Instance).Dependency = new object())
            };
			builder.RegisterModule(module);
			var container = builder.Build();

			var controller = (ModuleTestController)container.Resolve(module.IdentificationStrategy.ServiceForControllerType(typeof(ModuleTestController)));
			Assert.IsNotNull(controller.Dependency);
		}

		[Test]
		public void InjectsInvoker()
		{
			var builder = new ContainerBuilder();
			var module = new AutofacControllerModule(GetType().Assembly)
            {
				ActionInvokerType = typeof(TestActionInvoker)
            };
			builder.RegisterModule(module);
			var container = builder.Build();

			var controller = (ModuleTestController)container.Resolve(module.IdentificationStrategy.ServiceForControllerType(typeof(ModuleTestController)));
            Assert.IsInstanceOf<TestActionInvoker>(controller.ActionInvoker);
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
	}
}