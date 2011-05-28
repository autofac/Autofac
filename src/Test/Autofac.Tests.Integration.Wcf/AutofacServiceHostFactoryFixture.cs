using System;
using System.ServiceModel;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacServiceHostFactoryFixture
    {
        static readonly Uri[] DummyEndpoints = new[] { new Uri("http://localhost") };

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
            builder.Register<ITestService>(c => new TestService()).Named<object>("service");
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new AutofacServiceHostFactory();
                factory.CreateServiceHost("service", DummyEndpoints);
            });
        }

        [Test]
        public void ExecutesHostConfigurationActionWhenSet()
        {
            try
            {
                ServiceHostBase hostParameter = null;
                ServiceHostBase actualHost = null;
                bool actionCalled = false;

                AutofacHostFactory.HostConfigurationAction = host =>
                    {
                        hostParameter = host;
                        actionCalled = true;
                    };

                var builder = new ContainerBuilder();
                builder.RegisterType<object>();
                TestWithHostedContainer(builder.Build(), () =>
                {
                    var factory = new AutofacServiceHostFactory();
                    actualHost = factory.CreateServiceHost(typeof(object).FullName, DummyEndpoints);
                    Assert.IsNotNull(actualHost);
                });

                Assert.AreSame(hostParameter, actualHost);
                Assert.IsTrue(actionCalled);
            }
            finally
            {
                AutofacHostFactory.HostConfigurationAction = null;
            }
        }

        static void TestWithHostedContainer(IContainer container, Action test)
        {
            AutofacHostFactory.Container = container;
            try
            {
                test();
            }
            finally
            {
                AutofacHostFactory.Container = null;
            }
        }
    }
}
