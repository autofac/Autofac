using System;
using Autofac;
using AutofacContrib.Multitenant.Wcf;
using NUnit.Framework;
using Autofac.Core;

namespace AutofacContrib.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class AutofacServiceHostFactoryFixture
    {
        static readonly Uri[] DummyEndpoints = new[] { new Uri("http://localhost") };

        [SetUp]
        public void SetUp()
        {
            AutofacHostFactory.Container = null;
            AutofacHostFactory.HostConfigurationAction = null;
            AutofacHostFactory.ServiceImplementationDataProvider = new SimpleServiceImplementationDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            AutofacHostFactory.Container = null;
            AutofacHostFactory.HostConfigurationAction = null;
            AutofacHostFactory.ServiceImplementationDataProvider = null;
        }

        [Test]
        public void HostsKeyedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named<object>("service");
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new AutofacServiceHostFactory();
                var host = factory.CreateServiceHost("service", DummyEndpoints);
                Assert.AreEqual(typeof(object), host.Description.ServiceType);
                Assert.IsNotNull(host);
            });
        }

        [Test]
        public void HostsTypedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new AutofacServiceHostFactory();
                var host = factory.CreateServiceHost(typeof(object).FullName, DummyEndpoints);
                Assert.IsNotNull(host);
                Assert.AreEqual(typeof(object), host.Description.ServiceType);
            });
        }

        [Test]
        public void HostsTypedServicesAsServices()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => "Test").As<object>();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new AutofacServiceHostFactory();
                var host = factory.CreateServiceHost(typeof(object).FullName, DummyEndpoints);
                Assert.IsNotNull(host);
                Assert.AreEqual(typeof(string), host.Description.ServiceType);
            });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DetectsUnknownImplementationTypes()
        {
            var builder = new ContainerBuilder();
            builder.Register<IServiceProvider>(c => (Container)new ContainerBuilder().Build()).Named<object>("service");
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new AutofacServiceHostFactory();
                var host = factory.CreateServiceHost("service", DummyEndpoints);
            });
        }

        void TestWithHostedContainer(IContainer container, Action test)
        {
            AutofacServiceHostFactory.Container = container;
            try
            {
                test();
            }
            finally
            {
                AutofacServiceHostFactory.Container = null;
            }
        }
    }
}
