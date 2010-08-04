using System;
using System.Linq;
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
                b.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                    .AssignableTo(typeof(IMyService)));

            Assert.AreEqual(1, container.ComponentRegistry.Registrations.Count());
            Object obj;
            Assert.IsTrue(container.TryResolve(typeof(MyComponent), out obj));
            Assert.IsFalse(container.TryResolve(typeof(MyComponent2), out obj));
        }

        [Test]
        public void WhenAReleaseActionIsSupplied_TheComponentIsNotDisposedAutomatically()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>()
                .OnRelease(_ => { });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.IsFalse(dt.IsDisposed);
        }

        [Test]
        public void WhenAReleaseActionIsSupplied_TheInstanceIsPassedToTheReleaseAction()
        {
            var builder = new ContainerBuilder();
            object instance = null;
            builder.RegisterType<DisposeTracker>()
                .OnRelease(i => { instance = i; });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.AreSame(dt, instance);
        }
    }
}
