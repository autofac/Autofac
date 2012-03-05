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
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class AutofacControllerActivatorFixture
    {
        [Test]
        public void CreateThrowsExceptionIfLifetimeScopePropertyMissing()
        {
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var activator = new AutofacControllerActivator();

            var exception = Assert.Throws<InvalidOperationException>(
                () => activator.Create(controllerContext, typeof(ApiController)));

            var expectedException = AutofacControllerActivator.GetInvalidOperationException();
            Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
        }

        [Test]
        public void CreateThrowsExceptionIfLifetimeScopeInstanceMissing()
        {
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var activator = new AutofacControllerActivator();

            requestMessage.Properties.Add(AutofacControllerFactory.ApiRequestTag, null);

            var exception = Assert.Throws<InvalidOperationException>(
                () => activator.Create(controllerContext, typeof(TestController)));

            var expectedException = AutofacControllerActivator.GetInvalidOperationException();
            Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
        }

        [Test]
        public void CreateReturnsNullWhenTypeNotRegistered()
        {
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var builder = new ContainerBuilder();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var activator = new AutofacControllerActivator();

            requestMessage.Properties.Add(AutofacControllerFactory.ApiRequestTag, builder.Build());

            var controller = activator.Create(controllerContext, typeof(TestController));

            Assert.That(controller, Is.Null);
        }

        [Test]
        public void CreateResolvesControllerFromLifetimeScope()
        {
            var configuration = new HttpConfiguration();
            var routeData = new Mock<IHttpRouteData>();
            var requestMessage = new HttpRequestMessage();
            var builder = new ContainerBuilder();
            var controllerContext = new HttpControllerContext(configuration, routeData.Object, requestMessage);
            var activator = new AutofacControllerActivator();
            var expectedController = new TestController();

            builder.RegisterInstance(expectedController);
            requestMessage.Properties.Add(AutofacControllerFactory.ApiRequestTag, builder.Build());

            var controller = activator.Create(controllerContext, typeof(TestController));

            Assert.That(controller, Is.SameAs(expectedController));
        }
    }
}