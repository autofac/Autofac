using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationSettingsReaderFixture
    {
        [Test]
        public void Load_AllowsMultipleRegistrationsOfSameType()
        {
            var builder = ConfigureContainer("SameTypeRegisteredMultipleTimes");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.AreEqual(2, collection.Count(), "The wrong number of items were registered.");

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.IsTrue(collection.Any(a => a.Input == 5), "The first registration (5) wasn't found.");
            Assert.IsTrue(collection.Any(a => a.Input == 10), "The second registration (10) wasn't found.");
        }

        [Test]
        public void Load_IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer").Build();
            container.AssertRegistered<object>("a", "The component from the config file with the specified section name was not registered.");
            container.AssertRegistered<object>("b", "The component from the config file with the default section name was not registered.");
            container.AssertRegistered<object>("c", "The component from the referenced raw XML configuration file was not registered.");
        }

        [Test]
        public void Load_RegistersMetadata()
        {
            var container = ConfigureContainer("ComponentWithMetadata").Build();
            IComponentRegistration registration;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out registration), "The expected service wasn't registered.");
            Assert.AreEqual(42, (int)registration.Metadata["answer"], "The metadata on the registered component was not properly parsed.");
        }

        [Test]
        public void Load_SingletonWithTwoServices()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            container.AssertRegistered<ITestComponent>();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<SimpleComponent>();
            Assert.AreSame(container.Resolve<ITestComponent>(), container.Resolve<object>());
        }

        [Test]
        public void ParametersProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.AreEqual(1, cpt.Input);
        }

        [Test]
        public void PropertiesProvided()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
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
            builder.RegisterCollection<ITestComponent>("ia")
                    .As<IList<ITestComponent>>();
            var container = builder.Build();
            var collection = container.Resolve<IList<ITestComponent>>();
            var first = collection[0];
            Assert.IsInstanceOf(typeof(D), first);
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

        private static ContainerBuilder ConfigureContainer(string configFileBaseName)
        {
            var cb = new ContainerBuilder();
            var fullFilename = "Files/" + configFileBaseName + ".config";
            var csr = new ConfigurationSettingsReader(SectionHandler.DefaultSectionName, fullFilename);
            cb.RegisterModule(csr);
            return cb;
        }

        interface ITestComponent { }

        class SimpleComponent : ITestComponent
        {
            public SimpleComponent(int input) { Input = input; }

            public int Input { get; set; }

            public string Message { get; set; }
        }

        class B { }

        class C
        {
            public bool ABool { get; set; }
        }

        class D : ITestComponent { }

        class E
        {
            public B B { get; set; }
        }
    }
}
