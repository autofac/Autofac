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
        public void Load_ConstructorInjection()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.AreEqual(1, cpt.Input);
        }

        [Test]
        public void Load_ExternalOwnership()
        {
            var container = ConfigureContainer("ExternalOwnership").Build();
            IComponentRegistration registration;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(SimpleComponent)), out registration), "The expected component was not registered.");
            Assert.AreEqual(InstanceOwnership.ExternallyOwned, registration.Ownership, "The ownership was not set correctly.");
        }

        [Test]
        public void Load_IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer").Build();
            container.AssertRegisteredNamed<object>("a", "The component from the config file with the specified section name was not registered.");
            container.AssertRegisteredNamed<object>("b", "The component from the config file with the default section name was not registered.");
            container.AssertRegisteredNamed<object>("c", "The component from the referenced raw XML configuration file was not registered.");
        }

        [Test]
        public void Load_LifetimeScope_InstancePerDependency()
        {
            var container = ConfigureContainer("InstancePerDependency").Build();
            Assert.AreNotSame(container.Resolve<SimpleComponent>(), container.Resolve<SimpleComponent>(), "The component was not correctly registered with factory scope.");
        }

        [Test]
        public void Load_LifetimeScope_Singleton()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            Assert.AreSame(container.Resolve<ITestComponent>(), container.Resolve<ITestComponent>(), "The component was not correctly registered with singleton scope.");
        }

        [Test]
        public void Load_MemberOf()
        {
            var builder = ConfigureContainer("MemberOf");
            builder.RegisterCollection<ITestComponent>("named-collection").As<IList<ITestComponent>>();
            var container = builder.Build();
            var collection = container.Resolve<IList<ITestComponent>>();
            var first = collection[0];
            Assert.IsInstanceOf<SimpleComponent>(first, "The resolved collection member was the wrong type.");
        }

        [Test]
        public void Load_PropertyInjectionEnabledOnComponent()
        {
            var builder = ConfigureContainer("EnablePropertyInjection");
            builder.RegisterType<SimpleComponent>().As<ITestComponent>();
            var container = builder.Build();
            var e = container.Resolve<ComponentConsumer>();
            Assert.IsNotNull(e.Component, "The component was not injected into the property.");
        }

        [Test]
        public void Load_PropertyInjectionWithProvidedValues()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.AreEqual("hello", cpt.Message, "The string property value was not populated.");
            Assert.IsTrue(cpt.ABool, "The Boolean property value was not properly parsed/converted.");
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
        public void Load_SingleComponentWithTwoServices()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            container.AssertRegistered<ITestComponent>("The ITestComponent wasn't registered.");
            container.AssertRegistered<object>("The object wasn't registered.");
            container.AssertNotRegistered<SimpleComponent>("The base SimpleComponent type was incorrectly registered.");
            Assert.AreSame(container.Resolve<ITestComponent>(), container.Resolve<object>(), "Unable to resolve the singleton service on its two different registered interfaces.");
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
            public SimpleComponent() { }

            public SimpleComponent(int input) { Input = input; }

            public int Input { get; set; }

            public string Message { get; set; }

            public bool ABool { get; set; }
        }

        class ComponentConsumer
        {
            public ITestComponent Component { get; set; }
        }
    }
}
