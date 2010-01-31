using System.Collections.Generic;
using Autofac.Configuration;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationSettingsReaderFixture
    {
        [Test]
        public void ReadsMetadata()
        {
            var container = ConfigureContainer("Metadata").Build();
            IComponentRegistration registration;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new NamedService("a", typeof(object)), out registration));
            Assert.AreEqual(42, (int)registration.Metadata["answer"]);
        }

        [Test]
        public void IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer").Build();
            container.AssertRegistered<object>("a");
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
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            container.AssertRegistered<IA>();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<A>();
            Assert.AreSame(container.Resolve<IA>(), container.Resolve<object>());
        }

        [Test]
        public void ParametersProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (A)container.Resolve<IA>();
            Assert.AreEqual(1, cpt.I);
        }

        [Test]
        public void PropertiesProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (A)container.Resolve<IA>();
            Assert.AreEqual("hello", cpt.Message);
        }

        class B { }

        [Test]
        public void FactoryScope()
        {
            var container = ConfigureContainer("BPerDependency").Build();
            Assert.AreNotSame(container.Resolve<B>(), container.Resolve<B>());
        }

        class C
        {
            public bool ABool { get; set; }
        }

        [Test]
        public void ConfiguresBooleanProperties()
        {
            var container = ConfigureContainer("CWithBoolean").Build();
            var c = container.Resolve<C>();
            Assert.IsTrue(c.ABool);
        }

        [Test]
        public void SetsExternalOwnership()
        {
            var container = ConfigureContainer("BExternal").Build();
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
            var builder = ConfigureContainer("EWithPropertyInjection");
            builder.RegisterType<B>();
            var container = builder.Build();
            var e = container.Resolve<E>();
            Assert.IsNotNull(e.B);
        }

        class D : IA { }

        [Test]
        public void ConfiguresMemberOf()
        {
            var builder = ConfigureContainer("MemberOf");
            builder.RegisterCollection<IA>("ia")
                    .As<IList<IA>>();
            var container = builder.Build();
            var collection = container.Resolve<IList<IA>>();
            var first = collection[0];
            Assert.IsInstanceOf(typeof(D), first);
        }

        static ContainerBuilder ConfigureContainer(string configFile)
        {
            var cb = new ContainerBuilder();
            var fullFilename = configFile + ".config";
            var csr = new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, fullFilename);
            cb.RegisterModule(csr);
            return cb;
        }
    }
}
