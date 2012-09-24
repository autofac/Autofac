using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

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
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out registration));
            Assert.AreEqual(42, (int)registration.Metadata["answer"]);
        }

        [Test]
        public void IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer").Build();
            container.AssertRegistered<object>("a");
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

        [Test]
        public void FactoryScope()
        {
            var container = ConfigureContainer("BPerDependency").Build();
            Assert.AreNotSame(container.Resolve<B>(), container.Resolve<B>());
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

        [Test]
        public void SetsPropertyInjection()
        {
            var builder = ConfigureContainer("EWithPropertyInjection");
            builder.RegisterType<B>();
            var container = builder.Build();
            var e = container.Resolve<E>();
            Assert.IsNotNull(e.B);
        }

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

        [Test]
        public void AllowsMultipleRegistrationsOfSameType()
        {
            var builder = ConfigureContainer("TypeMultipleTimes");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<X>>();
            Assert.AreEqual(2, collection.Count());
            Assert.AreEqual("Bar", collection.ElementAt(0).Name);
            Assert.AreEqual("Foo", collection.ElementAt(1).Name);
        }

        [Test]
        public void ConfigurationCanBeXmlReader()
        {
            // This is the same as the Metadata.config but without the app.config wrapper.
            const string metadataConfiguration =
@"<autofac defaultAssembly=""mscorlib"">
  <components>
    <component type=""System.Object"" service=""System.Object"" name=""a"">
      <metadata>
        <item name=""answer"" value=""42"" type=""System.Int32"" />
      </metadata>
    </component>
  </components>
</autofac>";
            var cb = new ContainerBuilder();
            using (var reader = new XmlTextReader(new StringReader(metadataConfiguration)))
            {
                var csr = new ConfigurationSettingsReader(reader);
                cb.RegisterModule(csr);
            }
            var container = cb.Build();
            IComponentRegistration registration;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out registration));
            Assert.AreEqual(42, (int)registration.Metadata["answer"]);
        }

        static ContainerBuilder ConfigureContainer(string configFile)
        {
            var cb = new ContainerBuilder();
            var fullFilename = configFile + ".config";
            var csr = new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, fullFilename);
            cb.RegisterModule(csr);
            return cb;
        }

        interface IA { }

        class A : IA
        {
            public A(int i) { I = i; }

            public int I { get; set; }

            public string Message { get; set; }
        }

        class B { }

        class C
        {
            public bool ABool { get; set; }
        }

        class D : IA { }

        class E
        {
            public B B { get; set; }
        }

        class X
        {
            public string Name { get; set; }
            public X(string name) { Name = name; }
        }
    }
}
