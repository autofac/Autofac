using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Integration.Wcf;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public class AutofacServiceHostFactoryFixture
    {
        static readonly Uri[] DummyEndpoints = new[] { new Uri("http://localhost") };

        [Test]
        public void HostsNamedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named("service");
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
            builder.Register<IServiceProvider>(c => new Container()).Named("service");
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
