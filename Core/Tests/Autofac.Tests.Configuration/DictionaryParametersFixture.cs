using System.Collections;
using System.Collections.Generic;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class DictionaryParametersFixture
    {
        public class A
        {
            public IDictionary<string, string> Dictionary { get; set; }
        }

        [Test]
        public void InjectsDictionaryProperty()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<A>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.ContainsKey("1"));
            Assert.IsTrue(poco.Dictionary.ContainsKey("2"));
            Assert.AreEqual("Val1", poco.Dictionary["1"]);
            Assert.AreEqual("Val2", poco.Dictionary["2"]);
        }

        public class B
        {
            public IDictionary<string, string> Dictionary { get; set; }

            public B(IDictionary<string, string> dictionary)
            {
                Dictionary = dictionary;
            }
        }

        [Test]
        public void InjectsDictionaryParameter()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<B>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.ContainsKey("1"));
            Assert.IsTrue(poco.Dictionary.ContainsKey("2"));
            Assert.AreEqual("Val1", poco.Dictionary["1"]);
            Assert.AreEqual("Val2", poco.Dictionary["2"]);
        }

        public class C
        {
            public IDictionary Dictionary { get; set; }
        }

        [Test]
        public void InjectsNonGenericDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<C>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.Contains("1"));
            Assert.IsTrue(poco.Dictionary.Contains("2"));
            Assert.AreEqual("Val1", poco.Dictionary["1"]);
            Assert.AreEqual("Val2", poco.Dictionary["2"]);
        }
        
        public class D
        {
            public Dictionary<string, string> Dictionary { get; set; }
        }

        [Test]
        public void InjectsConcreteDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<D>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.ContainsKey("1"));
            Assert.IsTrue(poco.Dictionary.ContainsKey("2"));
            Assert.AreEqual("Val1", poco.Dictionary["1"]);
            Assert.AreEqual("Val2", poco.Dictionary["2"]);
        }

        public class E
        {
            public IDictionary<int, string> Dictionary { get; set; }
        }

        [Test]
        public void ConvertsDictionaryKey()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<E>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.ContainsKey(1));
            Assert.IsTrue(poco.Dictionary.ContainsKey(2));
            Assert.AreEqual("Val1", poco.Dictionary[1]);
            Assert.AreEqual("Val2", poco.Dictionary[2]);
        }

        public class F
        {
            public IDictionary<string, int> Dictionary { get; set; }
        }

        [Test]
        public void ConvertsDictionaryValue()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<F>();

            Assert.IsTrue(poco.Dictionary.Count == 2);
            Assert.IsTrue(poco.Dictionary.ContainsKey("1"));
            Assert.IsTrue(poco.Dictionary.ContainsKey("2"));
            Assert.AreEqual(1, poco.Dictionary["1"]);
            Assert.AreEqual(2, poco.Dictionary["2"]);
        }

        public class G
        {
            public IDictionary<string, string> Dictionary { get; set; }
        }

        [Test]
        public void InjectsEmptyDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<G>();

            Assert.IsNotNull(poco.Dictionary);
            Assert.IsTrue(poco.Dictionary.Count == 0);
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