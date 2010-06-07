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
    public class AutofacInstanceProviderFixture
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

        [Test(Description = "Attempts to create an instance provider with no service data.")]
        public void Ctor_NullServiceImplementationData()
        {
            Assert.Throws<ArgumentNullException>(() => new AutofacInstanceProvider(null));
        }

        [Test(Description = "Ensures you can't get an instance of a service given a null implementation resolver.")]
        public void GetInstance_NullResolver()
        {
            var data = this.BuildTestData();
            data.ImplementationResolver = null;
            var provider = new AutofacInstanceProvider(data);
            Assert.Throws<InvalidOperationException>(() => provider.GetInstance(null));
            Assert.Throws<InvalidOperationException>(() => provider.GetInstance(null, null));
        }

        [Test(Description = "Validates that the resolved service implementation type is checked for null before being returned to the host.")]
        public void GetInstance_NullServiceImplementationResolved()
        {
            var data = this.BuildTestData();
            data.ImplementationResolver = l => null;
            var provider = new AutofacInstanceProvider(data);
            Assert.Throws<InvalidOperationException>(() => provider.GetInstance(null));
            Assert.Throws<InvalidOperationException>(() => provider.GetInstance(null, null));
        }

        [Test(Description = "The service implementation object should be resolved from the request lifetime of the static container provider.")]
        public void GetInstance_ServiceImplementationResolvedFromContainerProvider()
        {
            var data = this.BuildTestData();
            var provider = new AutofacInstanceProvider(data);
            var instance1 = provider.GetInstance(null);
            var instance2 = provider.GetInstance(null, null);
            Assert.IsNotNull(instance1, "The single-parameter overload of GetInstance did not resolve a service implementation object.");
            Assert.IsNotNull(instance2, "The two-parameter overload of GetInstance did not resolve a service implementation object.");
            Assert.IsInstanceOf<ServiceImplementation>(instance1, "The single-parameter overload of GetInstance resolved the wrong type of service implementation object.");
            Assert.IsInstanceOf<ServiceImplementation>(instance2, "The two-parameter overload of GetInstance resolved the wrong type of service implementation object.");
        }

        [Test(Description = "Releasing the service implementation instance should also end the request lifetime.")]
        public void ReleaseInstance_EndsRequestLifetime()
        {
            var data = this.BuildTestData();
            var provider = new AutofacInstanceProvider(data);
            provider.ReleaseInstance(null, null);
            Assert.IsTrue(((TestContainerProvider)AutofacHostFactory.ContainerProvider).RequestLifetimeDisposed, "The request lifetime should end and be disposed when the service instance is released.");
        }

        private IContainerProvider BuildContainerProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceImplementation>().As<IServiceContract>().InstancePerDependency();
            return new TestContainerProvider() { ApplicationContainer = builder.Build() };
        }

        private ServiceImplementationData BuildTestData()
        {
            return new ServiceImplementationData()
                {
                    ConstructorString = typeof(IServiceContract).AssemblyQualifiedName,
                    ServiceTypeToHost = typeof(ServiceImplementation),
                    ImplementationResolver = l => l.Resolve<IServiceContract>()
                };
        }

        private class TestContainerProvider : IContainerProvider
        {
            public bool RequestLifetimeDisposed { get; set; }
            public IContainer ApplicationContainer { get; set; }

            public void EndRequestLifetime()
            {
                this.RequestLifetime.Dispose();
                this.RequestLifetime = null;
                this.RequestLifetimeDisposed = true;
            }

            private ILifetimeScope _requestLifetime = null;
            public ILifetimeScope RequestLifetime
            {
                get
                {
                    if (_requestLifetime == null)
                    {
                        _requestLifetime = this.ApplicationContainer.BeginLifetimeScope();
                        this.RequestLifetimeDisposed = false;
                    }
                    return _requestLifetime;
                }
                set
                {
                    _requestLifetime = value;
                    this.RequestLifetimeDisposed = false;
                }
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
