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

using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Autofac.Core;
using Autofac.Core.Lifetime;
using NUnit.Framework;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void ConfigureWebApiRegistersConfiguration()
        {
            AssertConfigureWebApiRegistration<HttpConfiguration, HttpConfiguration>();
        }

        [Test]
        public void ConfigureWebApiRegistersControllerActivator()
        {
            AssertConfigureWebApiRegistration<IHttpControllerActivator, AutofacControllerActivator>();
        }

        [Test]
        public void ConfigureWebApiRegistersControllerFactory()
        {
            AssertConfigureWebApiRegistration<IHttpControllerFactory, AutofacControllerFactory>();
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
        public void InstancePerApiRequestTagsRegistrations()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType(typeof(object)).InstancePerApiRequest();

            var container = builder.Build();
            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
            Assert.Throws<DependencyResolutionException>(() => container.BeginLifetimeScope().Resolve<object>());

            var apiRequestScope = container.BeginLifetimeScope(AutofacControllerFactory.ApiRequestTag);
            Assert.That(apiRequestScope.Resolve<object>(), Is.Not.Null);
        }

        static void AssertConfigureWebApiRegistration<TService, TInstance>()
        {
            var configuration = new HttpConfiguration();
            var builder = new ContainerBuilder();
            var service = new TypedService(typeof(TService));

            builder.ConfigureWebApi(configuration);

            var container = builder.Build();
            IComponentRegistration registration;
            Assert.That(container.ComponentRegistry.TryGetRegistration(service, out registration));

            var sharedInstance = registration.Sharing == InstanceSharing.Shared;
            var rootScopeLifetime = registration.Lifetime is RootScopeLifetime;
            Assert.That(sharedInstance && rootScopeLifetime, Is.True);

            Assert.That(container.Resolve<TService>(), Is.InstanceOf<TInstance>());
        }
    }
}