using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Configuration;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class EnumerableParametersFixture
    {
        public class A
        {
            public IList<string> List { get; set; }
        }

        [Test]
        public void PropertyStringListInjection()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<A>();

            Assert.IsTrue(poco.List.Count == 2);
            Assert.AreEqual(poco.List[0], "Val1");
            Assert.AreEqual(poco.List[1], "Val2");
        }

        public class B
        {
            public IList<int> List { get; set; }
        }

        [Test]
        public void ConvertsTypeInList()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<B>();

            Assert.IsTrue(poco.List.Count == 2);
            Assert.AreEqual(poco.List[0], 1);
            Assert.AreEqual(poco.List[1], 2);
        }

        public class C
        {
            public IList List { get; set; }
        }

        [Test]
        public void FillsNonGenericListWithString()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<C>();

            Assert.IsTrue(poco.List.Count == 2);
            Assert.AreEqual(poco.List[0], "1");
            Assert.AreEqual(poco.List[1], "2");
        }

        public class D
        {
            public int Num { get; set; }
        }

        [Test]
        public void InjectsSingleValueWithConversion()
        {
            //Assert.IsFalse("string" is IEnumerable);
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<D>();

            Assert.IsTrue(poco.Num == 123);
        }

        public class E
        {
            public IList<int> List { get; set; }

            public E(IList<int> list)
            {
                List = list;
            }
        }

        [Test]
        public void InjectsConstructorParameter()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<E>();

            Assert.IsTrue(poco.List.Count == 2);
            Assert.AreEqual(poco.List[0], 1);
            Assert.AreEqual(poco.List[1], 2);            
        }

        public class F
        {
            public IList<int> List { get; set; }
        }

        [Test]
        public void InjectsEmptyList()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<F>();

            Assert.IsNotNull(poco.List);
            Assert.AreEqual(0, poco.List.Count);
        }

        public class G
        {
            public IEnumerable Enumerable { get; set; }
        }

        [Test]
        public void InjectsIEnumerable()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<G>();

            Assert.IsNotNull(poco.Enumerable);
            var enumerable = poco.Enumerable.Cast<string>().ToList();
            Assert.IsTrue(enumerable.Count == 2);
            Assert.AreEqual(enumerable[0], "Val1");
            Assert.AreEqual(enumerable[1], "Val2");
        }

        public class H
        {
            public IEnumerable<int> Enumerable { get; set; }
        }

        [Test]
        public void InjectsGenericIEnumerable()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<H>();

            Assert.IsNotNull(poco.Enumerable);
            var enumerable = poco.Enumerable.ToList();
            Assert.IsTrue(enumerable.Count == 2);
            Assert.AreEqual(enumerable[0], 1);
            Assert.AreEqual(enumerable[1], 2);
        }

        public class I
        {
            public ICollection<int> Collection { get; set; }
        }

        [Test]
        public void InjectsGenericCollection()
        {
            var container = ConfigureContainer("EnumerableParameters").Build();

            var poco = container.Resolve<I>();

            Assert.IsNotNull(poco.Collection);
            Assert.IsTrue(poco.Collection.Count == 2);
            Assert.AreEqual(poco.Collection.First(), 1);
            Assert.AreEqual(poco.Collection.Last(), 2);
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
