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
            var cname = "s";

            builder.RegisterCollection<string>(cname)
                .As<IEnumerable<string>>();

            var s1 = "hello";
            var s2 = "world";

            builder.RegisterInstance(s1).MemberOf(cname);

            builder.RegisterInstance(s2).MemberOf(cname);

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
            var cname = "s";

            builder.RegisterCollection<string>(cname)
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
            var cname = "s";

            builder.RegisterCollection<string>(cname).As<string[]>();

            var s1 = "hello";

            builder.RegisterInstance(s1).MemberOf(cname);

            var container = builder.Build();

            Assert.AreEqual(s1, container.Resolve<string[]>()[0]);
        }

        [Test]
        public void CanRegisterCollectionWithoutGenericMethod()
        {
            var builder = new ContainerBuilder();
            var cname = "s";

            builder.RegisterCollection(cname, typeof(string))
                .As<string[]>();

            var container = builder.Build();

            Assert.IsTrue(container.IsRegistered<string[]>());
        }

        [Test]
        public void ServiceCanBelongToMultipleCollections()
        {
            var builder = new ContainerBuilder();
            string cname1 = "s", cname2 = "p";
            var element = "Hello";

            builder.RegisterCollection<string>(cname1);
            builder.RegisterCollection<string>(cname2);
            builder.RegisterInstance(element)
                .MemberOf(cname1)
                .MemberOf(cname2);
            
            var container = builder.Build();

            var coll1 = container.Resolve<string[]>();
            var coll2 = container.Resolve<string[]>();

            Assert.AreEqual(1, coll1.Count());
            Assert.AreEqual(1, coll2.Count());
        }
    }
}
