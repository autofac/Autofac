using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Configuration;
using NUnit.Framework;
using Autofac.Core;

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
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new NamedService("a"), out registration));
            Assert.AreEqual(42, (int)registration.ExtendedProperties["answer"]);
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
        public void ParametersProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices");
            var cpt = (A)container.Resolve<IA>();
            Assert.AreEqual(1, cpt.I);
        }

        [Test]
        public void PropertiesProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices");
            var cpt = (A)container.Resolve<IA>();
            Assert.AreEqual("hello", cpt.Message);
        }

        class B { }

        [Test]
        public void FactoryScope()
        {
            var container = ConfigureContainer("BPerDependency");
            Assert.AreNotSame(container.Resolve<B>(), container.Resolve<B>());
        }

        class C
        {
            public bool ABool { get; set; }
        }

        [Test]
        public void ConfiguresBooleanProperties()
        {
            var container = ConfigureContainer("CWithBoolean");
            var c = container.Resolve<C>();
            Assert.IsTrue(c.ABool);
        }

        [Test]
        public void SetsExternalOwnership()
        {
            var container = ConfigureContainer("BExternal");
            IComponentRegistration forB;
            container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(B)), out forB);
            Assert.AreEqual(InstanceOwnership.ExternallyOwned, forB.Ownership);
        }

        class E
        {
            public B B { get; set; }
        }

        [Test]
        public void SetsPropertyInjection()
        {
            var container = ConfigureContainer("EWithPropertyInjection");
            container.Configure(b => b.RegisterType<B>());
            var e = container.Resolve<E>();
            Assert.IsNotNull(e.B);
        }

        class D : IA { }

        [Test]
        public void ConfiguresMemberOf()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCollection<IA>()
                    .As<IList<IA>>()
                    .Named("collection");
            var container = builder.Build();
            
            ConfigureContainer(container, "MemberOf");

            var collection = container.Resolve<IList<IA>>();
            var first = collection[0];
            Assert.IsInstanceOf(typeof(D), first);
        }

        void ConfigureContainer(IContainer container, string configFile)
        {
            var fullFilename = configFile + ".config";
            var csr = new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, fullFilename);
            container.Configure(builder => builder.RegisterModule(csr));
        }

        IContainer ConfigureContainer(string configFile)
        {
            var container = new Container();
            ConfigureContainer(container, configFile);
            return container;
        }
    }
}
