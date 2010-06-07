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

namespace UnitTests.Autofac.Integration.Wcf
{
    [TestFixture]
    public class AutofacHostFactoryFixture
    {
        [SetUp]
        public void SetUp()
        {
            AutofacHostFactory.ContainerProvider = this.BuildContainerProvider(null);
        }

        [TearDown]
        public void TearDown()
        {
            AutofacHostFactory.ContainerProvider = null;
        }

        [Test(Description = "The default behavior when no different data provider is registered should be to create a service host given the contract type name.")]
        public void CreateServiceHost_CreatesHostUsingDefaultBehavior()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            Assert.IsNotNull(factory.CreateServiceHost(constructorString, baseAddresses), "No service host was created.");
        }

        [Test(Description = "Ensures you can't create a service host with an empty constructor string.")]
        public void CreateServiceHost_EmptyConstructorString()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = "";
            Assert.Throws<ArgumentException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null set of base addresses.")]
        public void CreateServiceHost_NullBaseAddresses()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = null;
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null constructor string.")]
        public void CreateServiceHost_NullConstructorString()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = null;
            Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null application container.")]
        public void CreateServiceHost_NullContainer()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            ((TestContainerProvider)AutofacHostFactory.ContainerProvider).ApplicationContainer = null;
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null container provider.")]
        public void CreateServiceHost_NullContainerProvider()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            AutofacHostFactory.ContainerProvider = null;
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Checks the validation for the resolved service type to host. Also verifies that the data provider can be substituted.")]
        public void CreateServiceHost_ResolvedServiceTypeToHostIsNotClass()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            var provider = this.BuildTestDataProvider(constructorString, typeof(IServiceContract), l => new ServiceImplementation());
            AutofacHostFactory.ContainerProvider = this.BuildContainerProvider(provider);
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Checks the validation for the resolved service type to host. Also verifies that the data provider can be substituted.")]
        public void CreateServiceHost_ResolvedServiceTypeToHostIsNull()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            var provider = this.BuildTestDataProvider(constructorString, null, l => new ServiceImplementation());
            AutofacHostFactory.ContainerProvider = this.BuildContainerProvider(provider);
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        private IContainerProvider BuildContainerProvider(IServiceImplementationDataProvider provider)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceImplementation>().As<IServiceContract>();
            if (provider != null)
            {
                builder.RegisterInstance(provider).As<IServiceImplementationDataProvider>();
            }
            return new TestContainerProvider() { ApplicationContainer = builder.Build() };
        }

        private IServiceImplementationDataProvider BuildTestDataProvider(string ctorString, Type toHost, Func<ILifetimeScope, object> resolver)
        {
            return new TestServiceImplementationDataProvider()
            {
                Data = new ServiceImplementationData()
                {
                    ConstructorString = ctorString,
                    ServiceTypeToHost = toHost,
                    ImplementationResolver = resolver
                }
            };
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

        private class TestHostFactory : AutofacHostFactory
        {
        }

        private class TestServiceImplementationDataProvider : IServiceImplementationDataProvider
        {
            public ServiceImplementationData Data { get; set; }

            public ServiceImplementationData GetServiceImplementationData(string constructorString)
            {
                return this.Data;
            }
        }

        [ServiceContract]
        private interface IServiceContract
        {
            [OperationContract]
            void Operation();
        }

        private class ServiceImplementation : IServiceContract
        {
            public void Operation()
            {
            }
        }
    }
}
