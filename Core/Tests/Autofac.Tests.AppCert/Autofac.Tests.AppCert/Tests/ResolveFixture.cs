using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Autofac.Features.Metadata;
using Autofac.Core.Registration;
using Autofac.Tests.AppCert.Testing;

namespace Autofac.Tests.AppCert.Tests
{
    [TestFixture]
    public class ResolveFixture
    {
        [Test]
        public void FailedResolve()
        {
            // Issue #376: MissingManifestResourceException thrown when a ComponentNotRegisteredException is thrown.
            var container = new ContainerBuilder().Build();
            Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<ISimpleService>());
        }

        [Test]
        public void SimpleResolve()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleService>().As<ISimpleService>();
            var container = builder.Build();
            var service = container.Resolve<ISimpleService>();
            Assert.AreEqual("Simple", service.ServiceValue, "The service did not resolve properly.");
        }

        [Test]
        public void ClassBasedMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleService>().As<ISimpleService>();
            var container = builder.Build();
            var service = container.Resolve<Meta<ISimpleService, Metadata>>();
            Assert.AreEqual(42, service.Metadata.TheInt, "Default metadata is missing from metadata view.");
        }

        public class Metadata
        {
            [DefaultValue(42)]
            public int TheInt { get; set; }
        }

        private interface ISimpleService
        {
            string ServiceValue { get; }
        }

        private class SimpleService : ISimpleService
        {
            public string ServiceValue
            {
                get
                {
                    return "Simple";
                }
            }
        }
    }
}
