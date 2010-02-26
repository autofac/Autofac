using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;
using System.Reflection;

namespace Autofac.Tests
{
    [TestFixture]
    public class RegistrationExtensionsTests
    {
        // ReSharper disable InconsistentNaming

        interface IMyService { }

        sealed class MyComponent : IMyService { }
        sealed class MyComponent2 {}

        [Test]
        public void RegistrationsMadeInConfigureExpressionAreAddedToContainer()
        {
            var ls = new Container()
                .BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<IMyService>();
            Assert.IsTrue(component is MyComponent);
        }

        [Test]
        public void OnlyServicesAssignableToASpecificTypeIsRegistered()
        {
            var container = new Container().BeginLifetimeScope(b =>
            {
                b.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AssignableTo(typeof(IMyService));
            });

            Assert.AreEqual(1, container.ComponentRegistry.Registrations.Count());
            Object obj;
            Assert.IsTrue(container.TryResolve(typeof(MyComponent), out obj));
            Assert.IsFalse(container.TryResolve(typeof(MyComponent2), out obj));
        }
    }
}
