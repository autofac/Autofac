// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.Collections.Generic;
using System.Linq;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class AutofacDependencyResolverFixture
    {
        [Test]
        public void NestedLifetimeScopeIsCreated()
        {
            IContainer container = GetContainer();
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container);

            Assert.That(resolver.CurrentLifetimeScope, Is.Not.Null);
        }

        [Test]
        public void NullConfigurationActionThrowsException()
        {
            IContainer container = GetContainer();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(container, null));

            Assert.That(exception.ParamName, Is.EqualTo("configurationAction"));
        }

        [Test]
        public void ConfigurationActionInvokedForNestedLifetime()
        {
            IContainer container = GetContainer();
            Action<ContainerBuilder> configurationAction = builder => builder.Register(c => new object());
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container, configurationAction);

            object service = resolver.GetService(typeof(object));
            IEnumerable<object> services = resolver.GetServices(typeof(object));

            Assert.That(service, Is.Not.Null);
            Assert.That(services.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetServiceReturnsNullForUnregisteredService()
        {
            IContainer container = GetContainer();
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container);

            object service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Null);
        }

        [Test]
        public void GetServiceReturnsRegisteredService()
        {
            IContainer container = GetContainer(builder => builder.Register(c => new object()));
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container);

            object service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
        {
            IContainer container = GetContainer();
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container);

            IEnumerable<object> services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetServicesReturnsRegisteredService()
        {
            IContainer container = GetContainer(builder => builder.Register(c => new object()));
            AutofacDependencyResolver resolver = new AutofacDependencyResolver(container);

            IEnumerable<object> services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(1));
        }

        static IContainer GetContainer(Action<ContainerBuilder> configurationAction = null)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new StubLifetimeScopeProvider()).As<ILifetimeScopeProvider>();
            if (configurationAction != null)
                configurationAction(builder);
            return builder.Build();
        }
    }

    public class StubLifetimeScopeProvider : ILifetimeScopeProvider
    {
        ILifetimeScope _lifetimeScope;

        public ILifetimeScope GetLifetimeScope(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
        {
            return _lifetimeScope ?? (_lifetimeScope = GetLifetimeScope(configurationAction, container));
        }

        static ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> requestLifetimeConfiguration, ILifetimeScope container)
        {
            return (requestLifetimeConfiguration == null)
                ? container.BeginLifetimeScope(RequestLifetimeModule.HttpRequestTag)
                : container.BeginLifetimeScope(RequestLifetimeModule.HttpRequestTag, requestLifetimeConfiguration);
        }
    }
}
