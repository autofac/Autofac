using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Util;
using Xunit;

namespace Autofac.Test
{
    public class RegistrationExtensionsTests
    {
        internal interface IMyService
        {
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }

        [Fact]
        public void RegistrationsMadeInConfigureExpressionAreAddedToContainer()
        {
            var ls = new Container()
                .BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<IMyService>();
            Assert.True(component is MyComponent);
        }

        [Fact]
        public void OnlyServicesAssignableToASpecificTypeAreRegisteredFromAssemblies()
        {
            var container = new Container().BeginLifetimeScope(b =>
                b.RegisterAssemblyTypes(GetType().GetTypeInfo().Assembly)
                    .AssignableTo(typeof(IMyService)));

            Assert.Equal(1, container.ComponentRegistry.Registrations.Count());
            Object obj;
            Assert.True(container.TryResolve(typeof(MyComponent), out obj));
            Assert.False(container.TryResolve(typeof(MyComponent2), out obj));
        }

        [Fact]
        public void OnlyServicesAssignableToASpecificTypeAreRegisteredFromTypeList()
        {
            var container = new Container().BeginLifetimeScope(b =>
                b.RegisterTypes(GetType().GetTypeInfo().Assembly.GetLoadableTypes().ToArray())
                    .AssignableTo(typeof(IMyService)));

            Assert.Equal(1, container.ComponentRegistry.Registrations.Count());
            Object obj;
            Assert.True(container.TryResolve(typeof(MyComponent), out obj));
            Assert.False(container.TryResolve(typeof(MyComponent2), out obj));
        }

        [Fact]
        public void WhenAReleaseActionIsSupplied_TheComponentIsNotDisposedAutomatically()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>()
                .OnRelease(_ => { });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.False(dt.IsDisposed);
        }

        [Fact]
        public void WhenAReleaseActionIsSupplied_TheInstanceIsPassedToTheReleaseAction()
        {
            var builder = new ContainerBuilder();
            object instance = null;
            builder.RegisterType<DisposeTracker>()
                .OnRelease(i => { instance = i; });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.Same(dt, instance);
        }

        [Fact]
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
            Assert.True(executed, "The release action should have executed.");
            Assert.False(provided.IsDisposed, "The release action should have superseded the automatic call to IDisposable.Dispose.");
        }

        public interface IImplementedInterface
        {
        }

        public class SelfComponent : IImplementedInterface
        {
        }

        [Fact]
        public void AsImplementedInterfaces_CanBeAppliedToNonGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(SelfComponent)).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface>();
        }

        [Fact]
        public void AsSelf_CanBeAppliedToNonGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(SelfComponent)).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent>();
        }

        [Fact]
        public void AsImplementedInterfaces_CanBeAppliedToInstanceRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new SelfComponent()).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface>();
        }

        internal IImplementedInterface SelfComponentFactory()
        {
            return new SelfComponent();
        }

        [Fact]
        public void AsImplementedInterfaces_CanBeAppliedToInstanceRegistrationsOfInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => SelfComponentFactory()).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface>();
        }

        [Fact]
        public void AsSelf_CanBeAppliedToInstanceRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new SelfComponent()).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent>();
        }

        public interface IImplementedInterface<T>
        {
        }

        public class SelfComponent<T> : IImplementedInterface<T>
        {
        }

        [Fact]
        public void AsImplementedInterfaces_CanBeAppliedToOpenGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IImplementedInterface<object>>();
        }

        [Fact]
        public void AsSelf_CanBeAppliedToOpenGenericRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsSelf();
            var context = builder.Build();

            context.Resolve<SelfComponent<object>>();
        }

        [Fact]
        public void AutoActivate_ResolvesComponentsAutomatically()
        {
            int singletonCount = 0;
            int instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent>().As<IMyService>().SingleInstance().AutoActivate().OnActivated(e => singletonCount++);
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.Equal(1, singletonCount);
            Assert.Equal(1, instanceCount);
        }

        [Fact]
        public void AutoActivate_MultipleAutoStartFlagsOnlyStartTheComponentOnce()
        {
            int instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().AutoActivate().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.Equal(1, instanceCount);
        }

        [Fact]
        public void AutoActivate_InvalidLifetimeConflictsWithAutoStart()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().InstancePerMatchingLifetimeScope("foo").AutoActivate();
            Assert.Throws<DependencyResolutionException>(() => builder.Build());
        }

        [Fact]
        public void AutoActivate_ContainerUpdateAutoActivatesNewComponents()
        {
            // Issue #454: ContainerBuilder.Update() doesn't activate AutoActivate components.
            int instanceCount = 0;
            var container = new ContainerBuilder().Build();
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => instanceCount++);
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618
            Assert.Equal(1, instanceCount);
        }

        [Fact]
        public void AutoActivate_ContainerUpdateDoesNotAutoActivateExistingComponents()
        {
            // Issue #454: ContainerBuilder.Update() shouldn't re-activate existing AutoActivate components.
            int firstCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => firstCount++);
            var container = builder.Build();
            Assert.Equal(1, firstCount);

            int secondCount = 0;
            var builder2 = new ContainerBuilder();
            builder2.RegisterType<MyComponent>().AutoActivate().OnActivated(e => secondCount++);
#pragma warning disable CS0618
            builder2.Update(container);
#pragma warning restore CS0618
            Assert.Equal(1, firstCount);
            Assert.Equal(1, secondCount);
        }

        [Fact]
        public void InstancePerRequest_AdditionalLifetimeScopeTagsCanBeProvided()
        {
            var builder = new ContainerBuilder();
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";
            builder.Register(c => new object()).InstancePerRequest(tag1, tag2);

            var container = builder.Build();

            var scope1 = container.BeginLifetimeScope(tag1);
            Assert.NotNull(scope1.Resolve<object>());

            var scope2 = container.BeginLifetimeScope(tag2);
            Assert.NotNull(scope2.Resolve<object>());

            var requestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            Assert.NotNull(requestScope.Resolve<object>());
        }

        [Fact]
        public void InstancePerRequest_ResolutionSucceedsInRequestLifetime()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType(typeof(object)).InstancePerRequest();

            var container = builder.Build();
            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
            Assert.Throws<DependencyResolutionException>(() => container.BeginLifetimeScope().Resolve<object>());

            var apiRequestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            Assert.NotNull(apiRequestScope.Resolve<object>());
        }
    }
}
