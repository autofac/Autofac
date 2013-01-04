using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Util;
using NUnit.Framework;

namespace Autofac.Tests
{
    [TestFixture]
    public class RegistrationExtensionsTests
    {
        // ReSharper disable InconsistentNaming

        interface IMyService { }

        public sealed class MyComponent : IMyService { }
        public sealed class MyComponent2 { }

        [Test]
        public void RegistrationsMadeInConfigureExpressionAreAddedToContainer()
        {
            var ls = new Container()
                .BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<IMyService>();
            Assert.IsTrue(component is MyComponent);
        }

        [Test]
        public void OnlyServicesAssignableToASpecificTypeAreRegisteredFromAssemblies()
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
        public void OnlyServicesAssignableToASpecificTypeAreRegisteredFromTypeList()
        {
            var container = new Container().BeginLifetimeScope(b =>
                b.RegisterTypes(Assembly.GetExecutingAssembly().GetLoadableTypes().ToArray())
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

        [Test]
        public void WhenAReleaseActionIsSupplied_AnActivatedProvidedInstanceWillExecuteReleaseAction()
        {
            var builder = new ContainerBuilder();
            var provided = new DisposeTracker();
            bool executed = false;
            builder.RegisterInstance(provided)
                .OnRelease(i => executed = true);
            var container = builder.Build();
            container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.IsTrue(executed, "The release action should have executed.");
            Assert.IsFalse(provided.IsDisposed, "The release action should have superseded the automatic call to IDisposable.Dispose.");
        }

        public interface IImplementedInterface { }
        public class SelfComponent : IImplementedInterface { }

        [Test]
        public void AsImplementedInterfaces_CanBeAppliedToNonGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(SelfComponent)).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface>();
        }

        [Test]
        public void AsSelf_CanBeAppliedToNonGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(SelfComponent)).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent>();
        }

        [Test]
        public void AsImplementedInterfaces_CanBeAppliedToInstanceRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new SelfComponent()).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface>();
        }

        [Test]
        public void AsSelf_CanBeAppliedToInstanceRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new SelfComponent()).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent>();
        }

        // ReSharper disable UnusedTypeParameter
        public interface IImplementedInterface<T> { }
        // ReSharper restore UnusedTypeParameter
        public class SelfComponent<T> : IImplementedInterface<T> { }

        [Test]
        public void AsImplementedInterfaces_CanBeAppliedToOpenGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface<object>>();
        }

        [Test]
        public void AsSelf_CanBeAppliedToOpenGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent<object>>();
        }

        [Test]
        public void AutoActivate_ResolvesComponentsAutomatically()
        {
            int singletonCount = 0;
            int instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent>().As<IMyService>().SingleInstance().AutoActivate().OnActivated(e => singletonCount++);
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.AreEqual(1, singletonCount, "The singleton component wasn't auto activated.");
            Assert.AreEqual(1, instanceCount, "The instance component wasn't auto activated.");
        }

        [Test]
        public void AutoActivate_MultipleAutoStartFlagsOnlyStartTheComponentOnce()
        {
            int instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().AutoActivate().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.AreEqual(1, instanceCount, "The instance component wasn't properly auto activated.");
        }

        [Test]
        public void AutoActivate_InvalidLifetimeConflictsWithAutoStart()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().InstancePerMatchingLifetimeScope("foo").AutoActivate();
            Assert.Throws<DependencyResolutionException>(() => builder.Build());
        }
    }
}
