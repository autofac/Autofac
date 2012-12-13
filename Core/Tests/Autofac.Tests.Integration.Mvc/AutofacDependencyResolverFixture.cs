// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacDependencyResolverFixture
    {
        [Test]
        public void CurrentPropertyExposesTheCorrectResolver()
        {
            var container = new ContainerBuilder().Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            DependencyResolver.SetResolver(resolver);

            Assert.That(AutofacDependencyResolver.Current, Is.EqualTo(DependencyResolver.Current));
        }

        [Test]
        public void NestedLifetimeScopeIsCreated()
        {
            var container = new ContainerBuilder().Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            Assert.That(resolver.RequestLifetimeScope, Is.Not.Null);
        }

        [Test]
        public void NullContainerThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(null));
            Assert.That(exception.ParamName, Is.EqualTo("container"));

            exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(null, cb => { }));
            Assert.That(exception.ParamName, Is.EqualTo("container"));

            exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(null, new Mock<ILifetimeScopeProvider>().Object));
            Assert.That(exception.ParamName, Is.EqualTo("container"));

            exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(null, new Mock<ILifetimeScopeProvider>().Object, cb => { }));
            Assert.That(exception.ParamName, Is.EqualTo("container"));
        }

        [Test]
        public void NullConfigurationActionThrowsException()
        {
            var container = new ContainerBuilder().Build();

            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(container, (Action<ContainerBuilder>)null));
            Assert.That(exception.ParamName, Is.EqualTo("configurationAction"));

            exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(container, new Mock<ILifetimeScopeProvider>().Object, null));
            Assert.That(exception.ParamName, Is.EqualTo("configurationAction"));
        }

        [Test]
        public void NullLifetimeScopeProviderThrowsException()
        {
            var container = new ContainerBuilder().Build();

            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(container, (ILifetimeScopeProvider)null));
            Assert.That(exception.ParamName, Is.EqualTo("lifetimeScopeProvider"));

            exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(container, null, cb => { }));
            Assert.That(exception.ParamName, Is.EqualTo("lifetimeScopeProvider"));
        }

        [Test]
        public void ApplicationContainerExposed()
        {
            var container = new ContainerBuilder().Build();
            var dependencyResolver = new AutofacDependencyResolver(container);

            Assert.That(dependencyResolver.ApplicationContainer, Is.EqualTo(container));
        }

        [Test]
        public void ConfigurationActionInvokedForNestedLifetime()
        {
            var container = new ContainerBuilder().Build();
            Action<ContainerBuilder> configurationAction = builder => builder.Register(c => new object());
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider, configurationAction);

            var service = resolver.GetService(typeof(object));
            var services = resolver.GetServices(typeof(object));

            Assert.That(service, Is.Not.Null);
            Assert.That(services.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetServiceReturnsNullForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            var service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Null);
        }

        [Test]
        public void GetServiceReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            var service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            var services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetServicesReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            var lifetimeScopeProvider = new StubLifetimeScopeProvider(container);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);

            var services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(1));
        }
    }
}
