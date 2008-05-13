using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Configuration;
using Autofac.Builder;
using System.IO;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationSettingsReaderFixture
    {
        [Test]
        public void ReadsExtendedProperties()
        {
            var container = ConfigureContainer("ExtendedProperties");
            IComponentRegistration registration;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(new NamedService("a"), out registration));
            Assert.AreEqual(42, (int)registration.Descriptor.ExtendedProperties["answer"]);
        }

        [Test]
        public void IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer");
            container.AssertRegistered("a");
        }

        interface IA { }

        class A : IA
        {
            public A(int i) { I = i; }

            public int I { get; set; }

            public string Message { get; set; }
        }

        [Test]
        public void SingletonWithTwoServices()
        {
            var container = ConfigureContainer("SingletonWithTwoServices");
            container.AssertRegistered<IA>();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<A>();
            Assert.AreSame(container.Resolve<IA>(), container.Resolve<object>());
        }

        [Test]
        public void ParametersAndPropertiesProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices");
            var cpt = (A)container.Resolve<IA>();
            Assert.AreEqual(1, cpt.I);
            Assert.AreEqual("hello", cpt.Message);
        }

        class B { }

        [Test]
        public void FactoryScope()
        {
            var container = ConfigureContainer("BFactory");
            Assert.AreNotSame(container.Resolve<B>(), container.Resolve<B>());
        }

        IContainer ConfigureContainer(string configFile)
        {
            var fullFilename =  @"Configuration\" + configFile + ".config";
            var csr = new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, fullFilename);
            var builder = new ContainerBuilder();
            builder.RegisterModule(csr);
            return builder.Build();
        }
    }
}
