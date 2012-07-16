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
        public void AsModelBinderForTypesThrowsExceptionWhenAllTypesNullInList()
        {
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentException>(() => registration.AsModelBinderForTypes(null, null, null));
        }

        [Test]
        public void AsModelBinderForTypesThrowsExceptionForEmptyTypeList()
        {
            var types = new Type[0];
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentException>(() => registration.AsModelBinderForTypes(types));
        }

        [Test]
        public void AsModelBinderForTypesRegistersInstanceModelBinder()
        {
            IDependencyResolver originalResolver = null;
            try
            {
                originalResolver = DependencyResolver.Current;
                var builder = new ContainerBuilder();
                var binder = new TestModelBinder();
                builder.RegisterInstance(binder).AsModelBinderForTypes(typeof(TestModel1));
                var container = builder.Build();
                DependencyResolver.SetResolver(new AutofacDependencyResolver(container, new StubLifetimeScopeProvider(container)));
                var provider = new AutofacModelBinderProvider();
                Assert.AreSame(binder, provider.GetBinder(typeof(TestModel1)));
            }
            finally
            {
                DependencyResolver.SetResolver(originalResolver);
            }
        }

        [Test]
        public void AsModelBinderForTypesRegistersTypeModelBinder()
        {
            IDependencyResolver originalResolver = null;
            try
            {
                originalResolver = DependencyResolver.Current;
                var builder = new ContainerBuilder();
                builder.RegisterType<TestModelBinder>().AsModelBinderForTypes(typeof(TestModel1), typeof(TestModel2));
                var container = builder.Build();
                DependencyResolver.SetResolver(new AutofacDependencyResolver(container, new StubLifetimeScopeProvider(container)));
                var provider = new AutofacModelBinderProvider();
                Assert.IsInstanceOf<TestModelBinder>(provider.GetBinder(typeof(TestModel1)), "The model binder was not registered for TestModel1");
                Assert.IsInstanceOf<TestModelBinder>(provider.GetBinder(typeof(TestModel2)), "The model binder was not registered for TestModel2");
            }
            finally
            {
                DependencyResolver.SetResolver(originalResolver);
            }
        }

        [Test]
        public void AsModelBinderForTypesThrowsExceptionForNullRegistration()
        {
            IRegistrationBuilder<RegistrationExtensionsFixture, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = null;
            Assert.Throws<ArgumentNullException>(() => registration.AsModelBinderForTypes(typeof(TestModel1)));
        }

        [Test]
        public void AsModelBinderForTypesThrowsExceptionForNullTypeList()
        {
            Type[] types = null;
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentNullException>(() => registration.AsModelBinderForTypes(types));
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
                .OnActivating(e => ((TestController)e.Instance).Dependency = new object());

            var container = builder.Build();

            var controller = container.Resolve<TestController>();
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

            var controller = container.Resolve<TestController>();
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

        [Test]
        public void AsActionFilterForControllerScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsActionFilterFor<TestController>(null));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsActionFilterForActionScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsActionFilterFor<TestController>
                    (null, c => c.Action1(Param<string>.Value)));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsActionFilterForControllerScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>();
            var container = builder.Build();

            var key = new FilterKey(typeof(TestController), FilterScope.Controller, null);

            Assert.That(container.IsRegisteredWithKey<IActionFilter>(key), Is.True);
        }

        [Test]
        public void AsActionFilterForActionScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(c => c.Action1(Param<string>.Value));
            var container = builder.Build();

            var methodInfo = TestController.GetAction1MethodInfo();
            var key = new FilterKey(typeof(TestController), FilterScope.Action, methodInfo);
            
            Assert.That(container.IsRegisteredWithKey<IActionFilter>(key), Is.True);
        }

        [Test]
        public void AsAuthoizationFilterForControllerScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsAuthorizationFilterFor<TestController>(null));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsAuthoizationFilterForActionScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsAuthorizationFilterFor<TestController>
                    (null, c => c.Action1(Param<string>.Value)));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsAuthoizationFilterForControllerScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>();
            var container = builder.Build();

            var key = new FilterKey(typeof(TestController), FilterScope.Controller, null);

            Assert.That(container.IsRegisteredWithKey<IAuthorizationFilter>(key), Is.True);
        }

        [Test]
        public void AsAuthoizationFilterForActionScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(c => c.Action1(Param<string>.Value));
            var container = builder.Build();

            var methodInfo = TestController.GetAction1MethodInfo();
            var key = new FilterKey(typeof(TestController), FilterScope.Action, methodInfo);

            Assert.That(container.IsRegisteredWithKey<IAuthorizationFilter>(key), Is.True);
        }

        [Test]
        public void AsExceptionFilterForControllerScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsExceptionFilterFor<TestController>(null));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsExceptionFilterForActionScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsExceptionFilterFor<TestController>
                    (null, c => c.Action1(Param<string>.Value)));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsExceptionFilterForControllerScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>();
            var container = builder.Build();

            var key = new FilterKey(typeof(TestController), FilterScope.Controller, null);

            Assert.That(container.IsRegisteredWithKey<IExceptionFilter>(key), Is.True);
        }

        [Test]
        public void AsExceptionFilterForActionScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(c => c.Action1(Param<string>.Value));
            var container = builder.Build();

            var methodInfo = TestController.GetAction1MethodInfo();
            var key = new FilterKey(typeof(TestController), FilterScope.Action, methodInfo);

            Assert.That(container.IsRegisteredWithKey<IExceptionFilter>(key), Is.True);
        }

        [Test]
        public void AsResultFilterForControllerScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsResultFilterFor<TestController>(null));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsResultFilterForActionScopedFilterThrowsExceptionForNullRegistration()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.Mvc.RegistrationExtensions.AsResultFilterFor<TestController>
                    (null, c => c.Action1(Param<string>.Value)));

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void AsResultFilterForControllerScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>();
            var container = builder.Build();

            var key = new FilterKey(typeof(TestController), FilterScope.Controller, null);

            Assert.That(container.IsRegisteredWithKey<IResultFilter>(key), Is.True);
        }

        [Test]
        public void AsResultFilterForActionScopedFilterAddsKeyedRegistration()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(c => c.Action1(Param<string>.Value));
            var container = builder.Build();

            var methodInfo = TestController.GetAction1MethodInfo();
            var key = new FilterKey(typeof(TestController), FilterScope.Action, methodInfo);

            Assert.That(container.IsRegisteredWithKey<IResultFilter>(key), Is.True);
        }
    }
}