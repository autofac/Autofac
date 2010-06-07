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
// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.ServiceModel;
using Autofac;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class DefaultServiceImplementationDataProviderFixture
    {
        [SetUp]
        public void SetUp()
        {
            AutofacHostFactory.ContainerProvider = this.BuildContainerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            AutofacHostFactory.ContainerProvider = null;
        }

        [Test(Description = "Ensures you can't get a service with a null constructor string.")]
        public void GetServiceImplementationData_EmptyConstructorString()
        {
            Assert.Throws<ArgumentException>(() => new DefaultServiceImplementationDataProvider().GetServiceImplementationData(""));
        }

        [Test(Description = "Checks the handling of a named service implementation.")]
        public void GetServiceImplementationData_NamedImplementation()
        {
            var data = new DefaultServiceImplementationDataProvider().GetServiceImplementationData("service");
            Assert.IsNotNull(data, "The returned data should not be null.");
            Assert.AreEqual("service", data.ConstructorString, "The constructor string should match what was passed in.");
            Assert.AreEqual(typeof(ServiceImplementation2), data.ServiceTypeToHost, "The named service type should be the type to host.");
            var impl = data.ImplementationResolver(AutofacHostFactory.ContainerProvider.RequestLifetime);
            Assert.IsInstanceOf<ServiceImplementation2>(impl, "The wrong service implementation type was resolved.");
        }

        [Test(Description = "Ensures you can't get a service with a null constructor string.")]
        public void GetServiceImplementationData_NullConstructorString()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultServiceImplementationDataProvider().GetServiceImplementationData(null));
        }

        [Test(Description = "Ensures you can't get a service with a null container;.")]
        public void GetServiceImplementationData_NullContainer()
        {
            ((TestContainerProvider)AutofacHostFactory.ContainerProvider).ApplicationContainer = null;
            Assert.Throws<InvalidOperationException>(() => new DefaultServiceImplementationDataProvider().GetServiceImplementationData(typeof(IServiceContract).AssemblyQualifiedName));
        }

        [Test(Description = "Ensures you can't get a service with a null container provider.")]
        public void GetServiceImplementationData_NullContainerProvider()
        {
            AutofacHostFactory.ContainerProvider = null;
            Assert.Throws<InvalidOperationException>(() => new DefaultServiceImplementationDataProvider().GetServiceImplementationData(typeof(IServiceContract).AssemblyQualifiedName));
        }

        [Test(Description = "Checks the handling of a named service implementation.")]
        public void GetServiceImplementationData_ServiceNotRegistered()
        {
            Assert.Throws<InvalidOperationException>(() => new DefaultServiceImplementationDataProvider().GetServiceImplementationData("no-such-service"));
        }

        [Test(Description = "Checks the handling of a typed service implementation.")]
        public void GetServiceImplementationData_TypedImplementation()
        {
            var data = new DefaultServiceImplementationDataProvider().GetServiceImplementationData(typeof(IServiceContract).AssemblyQualifiedName);
            Assert.IsNotNull(data, "The returned data should not be null.");
            Assert.AreEqual(typeof(IServiceContract).AssemblyQualifiedName, data.ConstructorString, "The constructor string should match what was passed in.");
            Assert.AreEqual(typeof(ServiceImplementation1), data.ServiceTypeToHost, "The named service type should be the type to host.");
            var impl = data.ImplementationResolver(AutofacHostFactory.ContainerProvider.RequestLifetime);
            Assert.IsInstanceOf<ServiceImplementation1>(impl, "The wrong service implementation type was resolved.");
        }

        private IContainerProvider BuildContainerProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceImplementation1>().As<IServiceContract>();
            builder.RegisterType<ServiceImplementation2>().Named<object>("service");
            return new TestContainerProvider() { ApplicationContainer = builder.Build() };
        }

        private class TestContainerProvider : IContainerProvider
        {
            public IContainer ApplicationContainer { get; set; }

            public void EndRequestLifetime()
            {
                this.RequestLifetime.Dispose();
                this.RequestLifetime = null;
            }

            private ILifetimeScope _requestLifetime = null;
            public ILifetimeScope RequestLifetime
            {
                get
                {
                    if (_requestLifetime == null)
                    {
                        _requestLifetime = this.ApplicationContainer.BeginLifetimeScope();
                    }
                    return _requestLifetime;
                }
                set
                {
                    _requestLifetime = value;
                }
            }
        }

        [ServiceContract]
        private interface IServiceContract
        {
            [OperationContract]
            void Operation();
        }

        private class ServiceImplementation1 : IServiceContract
        {
            public void Operation()
            {
            }
        }

        private class ServiceImplementation2 : IServiceContract
        {
            public void Operation()
            {
            }
        }
    }
}
