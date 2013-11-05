// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Autofac.Builder;
using Autofac.Core;
using NUnit.Framework;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void AdditionalLifetimeScopeTagsCanBeProvidedToInstancePerHttpRequest()
        {
            var builder = new ContainerBuilder();
            const string tag1 = "Tag1";
            const string tag2 = "Tag2";
            builder.Register(c => new object()).InstancePerApiRequest(tag1, tag2);

            var container = builder.Build();

            var scope1 = container.BeginLifetimeScope(tag1);
            Assert.That(scope1.Resolve<object>(), Is.Not.Null);

            var scope2 = container.BeginLifetimeScope(tag2);
            Assert.That(scope2.Resolve<object>(), Is.Not.Null);

            var requestScope = container.BeginLifetimeScope(AutofacWebApiDependencyResolver.ApiRequestTag);
            Assert.That(requestScope.Resolve<object>(), Is.Not.Null);
        }

        [Test]
        public void RegisterApiControllersRegistersTypesWithControllerSuffix()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.That(container.IsRegistered<TestController>(), Is.True);
        }

        [Test]
        public void RegisterApiControllersIgnoresTypesWithoutControllerSuffix()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.That(container.IsRegistered<IsAControllerNot>(), Is.False);
        }

        [Test]
        public void RegisterApiControllersFindsTypesImplemtingInterfaceOnly()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.That(container.IsRegistered<InterfaceController>(), Is.True);
        }

        [Test]
        public void InstancePerApiRequestRequiresControllerTypeParameter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterType<object>().InstancePerApiControllerType(null));

            Assert.That(exception.ParamName, Is.EqualTo("controllerType"));
        }

        [Test]
        public void InstancePerApiRequestTagsRegistrations()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType(typeof(object)).InstancePerApiRequest();

            var container = builder.Build();
            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
            Assert.Throws<DependencyResolutionException>(() => container.BeginLifetimeScope().Resolve<object>());

            var apiRequestScope = container.BeginLifetimeScope(AutofacWebApiDependencyResolver.ApiRequestTag);
            Assert.That(apiRequestScope.Resolve<object>(), Is.Not.Null);
        }

        [Test]
        public void RegisterWebApiModelBinderProviderThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.WebApi.RegistrationExtensions.RegisterWebApiModelBinderProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("builder"));
        }

        [Test]
        public void RegisterWebApiModelBinderProviderRegistersSingleInstanceProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterInstance(new HttpConfiguration());
            var container = builder.Build();

            var resolvedProvider1 = container.Resolve<ModelBinderProvider>();
            var resolvedProvider2 = container.Resolve<ModelBinderProvider>();

            Assert.That(resolvedProvider1, Is.SameAs(resolvedProvider2));
        }

        [Test]
        public void RegisterWebApiModelBindersThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.WebApi.RegistrationExtensions.RegisterWebApiModelBinders(null, Assembly.GetExecutingAssembly()));
            Assert.That(exception.ParamName, Is.EqualTo("builder"));
        }

        [Test]
        public void RegisterWebApiModelBindersThrowsExceptionForNullAssemblies()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ContainerBuilder().RegisterWebApiModelBinders(null));
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
            var builder = new ContainerBuilder();
            var binder = new TestModelBinder(new Dependency());
            builder.RegisterInstance(binder).AsModelBinderForTypes(typeof(TestModel1));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var provider = new AutofacWebApiModelBinderProvider();
            Assert.AreSame(binder, provider.GetBinder(configuration, typeof(TestModel1)));
        }

        [Test]
        public void AsModelBinderForTypesRegistersTypeModelBinder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>();
            builder.RegisterType<TestModelBinder>().AsModelBinderForTypes(typeof(TestModel1), typeof(TestModel2));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration {DependencyResolver = resolver};
            var provider = new AutofacWebApiModelBinderProvider();

            Assert.That(provider.GetBinder(configuration, typeof(TestModel1)), Is.InstanceOf<TestModelBinder>());
            Assert.That(provider.GetBinder(configuration, typeof(TestModel2)), Is.InstanceOf<TestModelBinder>());
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
        public void InstancePerApiControllerTypeRequiresTypeParameter()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterType<object>().InstancePerApiControllerType(null));

            Assert.That(exception.ParamName, Is.EqualTo("controllerType"));
        }

        [Test]
        public void InstancePerApiControllerTypeAddsKeyedRegistration()
        {
            var controllerType = typeof(TestController);
            var serviceKey = new ControllerTypeKey(controllerType);

            var builder = new ContainerBuilder();
            builder.RegisterType<object>().InstancePerApiControllerType(controllerType);
            var container = builder.Build();
            
            Assert.That(container.IsRegisteredWithKey<object>(serviceKey), Is.True);
        }

        [Test]
        public void RegisterHttpRequestMessageAddsHandler()
        {
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterHttpRequestMessage(config);

            Assert.That(config.MessageHandlers.OfType<CurrentRequestHandler>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void RegisterHttpRequestMessageThrowsGivenNullConfig()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterHttpRequestMessage(null));

            Assert.That(exception.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void RegisterHttpRequestMessageEnsuresHandlerAddedOnlyOnce()
        {
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            builder.RegisterHttpRequestMessage(config);
            builder.RegisterHttpRequestMessage(config);

            Assert.That(config.MessageHandlers.OfType<CurrentRequestHandler>().Count(), Is.EqualTo(1));
        }


        // Action filters

        [Test]
        public void AsActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsWebApiActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void AsActionFilterForServiceTypeMustBeActionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiActionFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        // Authorization filter

        [Test]
        public void AsAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>())).AsWebApiAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void AsWebApiAuthorizationFilterForServiceTypeMustBeAuthorizationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        // Exception filters

        [Test]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>())).AsWebApiExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void AsWebApiAuthorizationFilterForServiceTypeMustBeExceptionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        // Authentication filters

        [Test]
        public void AsAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>())).AsWebApiAuthenticationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void AsWebApiAuthenticationFilterForServiceTypeMustBeAuthenticationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthenticationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        // Action filter override

        [Test]
        public void OverrideActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        // Authorization filter override

        [Test]
        public void OverrideAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        // Exception filter override

        [Test]
        public void OverrideExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        // Authentication filter override

        [Test]
        public void OverrideAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiAuthenticationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }
    }
}