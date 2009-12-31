using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class CollectionRegistrationBuilderTests
    {
        [Test]
        public void RegisterCollection_Scenario()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>()
                .As<IEnumerable<string>>();

            var s1 = "hello";
            var s2 = "world";

            builder.RegisterInstance(s1)
                .MemberOf(typeof(IEnumerable<string>));

            builder.RegisterInstance(s2)
                .MemberOf(typeof(IEnumerable<string>));

            var container = builder.Build();

            var strings = container.Resolve<IEnumerable<string>>();

            Assert.AreEqual(2, strings.Count());
            Assert.IsTrue(strings.Contains(s1));
            Assert.IsTrue(strings.Contains(s2));
        }

        [Test]
        public void NewCollectionInNewContext()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>()
                .As<IEnumerable<string>>()
                .InstancePerLifetimeScope();

            var outer = builder.Build();

            var coll = outer.Resolve<IEnumerable<string>>();

            Assert.IsNotNull(coll);
            Assert.AreSame(coll, outer.Resolve<IEnumerable<string>>());

            var inner = outer.BeginLifetimeScope();

            Assert.AreNotSame(coll, inner.Resolve<IEnumerable<string>>());
        }

        [Test]
        public void CollectionRegistrationsSupportArrays()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>()
                .As<string[]>();

            var s1 = "hello";

            builder.RegisterInstance(s1).MemberOf(typeof(string[]));

            var container = builder.Build();

            Assert.AreEqual(s1, container.Resolve<string[]>()[0]);
        }

        [Test]
        public void CanRegisterCollectionWithoutGenericMethod()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection(typeof(string))
                .As<string[]>();

            var container = builder.Build();

            Assert.IsTrue(container.IsRegistered<string[]>());
        }

        [Test]
        public void ServiceCanBelongToMultipleCollections()
        {
            var builder = new ContainerBuilder();
            var element = "Hello";

            builder.RegisterCollection<string>().As<IEnumerable<string>>();
            builder.RegisterCollection<string>().As<ICollection<string>>();
            builder.RegisterInstance(element)
                .MemberOf(typeof(IEnumerable<string>))
                .MemberOf(typeof(ICollection<string>));
            
            var container = builder.Build();

            var coll1 = container.Resolve<IEnumerable<string>>();
            var coll2 = container.Resolve<ICollection<string>>();

            Assert.AreEqual(1, coll1.Count());
            Assert.AreEqual(1, coll2.Count());
        }
    }
}
