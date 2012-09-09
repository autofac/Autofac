using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Features.Indexed;
using NUnit.Framework;

namespace Autofac.Tests.Features.Indexed
{
    [TestFixture]
    public class KeyedServiceIndexTests
    {
        [Test]
        public void IndexerRetrievesComoponentsFromContextByKey()
        {
            var key = 42;
            var cpt = "Hello";

            var idx = CreateTarget(cpt, key);

            Assert.AreSame(cpt, idx[key]);
        }

        [Test]
        public void TryGetValueRetrievesComoponentsFromContextByKey()
        {
            var key = 42;
            var cpt = "Hello";

            var idx = CreateTarget(cpt, key);

            string val;

            Assert.IsTrue(idx.TryGetValue(key, out val));
            Assert.AreSame(cpt, val);
        }

        static KeyedServiceIndex<int, string> CreateTarget(string cpt, int key)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            return new KeyedServiceIndex<int, string>(container);
        }

        [Test]
        public void WhenKeyNotFound_IndexerReturnsFalse()
        {
            var key = 42;
            var cpt = "Hello";

            var idx = CreateTarget(cpt, key);

            string val;

            Assert.IsFalse(idx.TryGetValue(key + 1, out val));
        }
    }
}
