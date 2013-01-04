// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.SignalR
{
    [TestFixture]
    public class AutofacDependencyResolverFixture
    {
        [Test]
        public void CurrentPropertyExposesTheCorrectResolver()
        {
            var container = new ContainerBuilder().Build();
            var resolver = new AutofacDependencyResolver(container);

            GlobalHost.DependencyResolver = resolver;

            Assert.That(AutofacDependencyResolver.Current, Is.EqualTo(GlobalHost.DependencyResolver));
        }

        [Test]
        public void NullLifetimeScopeThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacDependencyResolver(null));
            Assert.That(exception.ParamName, Is.EqualTo("lifetimeScope"));
        }

        [Test]
        public void ProvidedLifetimeScopeExposed()
        {
            var container = new ContainerBuilder().Build();
            var dependencyResolver = new AutofacDependencyResolver(container);

            Assert.That(dependencyResolver.LifetimeScope, Is.EqualTo(container));
        }

        [Test]
        public void GetServiceReturnsNullForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            var resolver = new AutofacDependencyResolver(container);

            var service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Null);
        }

        [Test]
        public void GetServiceReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            var resolver = new AutofacDependencyResolver(container);

            var service = resolver.GetService(typeof(object));

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            var resolver = new AutofacDependencyResolver(container);

            var services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetServicesReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            var resolver = new AutofacDependencyResolver(container);

            var services = resolver.GetServices(typeof(object));

            Assert.That(services.Count(), Is.EqualTo(1));
        }

        [Test]
        public void CanResolveDefaultServices()
        {
            var container = new ContainerBuilder().Build();
            var resolver = new AutofacDependencyResolver(container);

            var service = resolver.GetService(typeof(IMessageBus));

            Assert.That(service, Is.InstanceOf<IMessageBus>());
        }

        [Test]
        public void CanOverrideDefaultServices()
        {
            var builder = new ContainerBuilder();
            var messageBus = new Mock<IMessageBus>().Object;
            builder.RegisterInstance(messageBus);
            var resolver = new AutofacDependencyResolver(builder.Build());

            var service = resolver.GetService(typeof(IMessageBus));

            Assert.That(service, Is.SameAs(messageBus));
        }
    }
}
