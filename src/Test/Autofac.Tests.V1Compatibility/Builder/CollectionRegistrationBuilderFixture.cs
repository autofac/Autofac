using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.V1Compatibility.Builder
{
    [TestFixture]
    public class CollectionRegistrationBuilderFixture
    {
        [Test]
        public void RegisterCollection()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>()
                .As<IEnumerable<string>>()
                .Named("my bag of string");

            var s1 = "hello";
            var s2 = "world";

            builder.Register(s1)
                .Named("my stringy string")
                .MemberOf(typeof(IEnumerable<string>));

            builder.Register(s2)
                .Named("my slick string")
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
                .WithScope(InstanceScope.Container);

            var outer = builder.Build();

            var coll = outer.Resolve<IEnumerable<string>>();

            Assert.IsNotNull(coll);
            Assert.AreSame(coll, outer.Resolve<IEnumerable<string>>());

            var inner = outer.CreateInnerContainer();

            Assert.AreNotSame(coll, inner.Resolve<IEnumerable<string>>());
        }

        [Test]
        public void NewMembersAddedInInnerContextDoNotAffectOuter()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>().ContainerScoped();

            var container = builder.Build();

            var inner = container.CreateInnerContainer();

            var innerBuilder = new ContainerBuilder();
            innerBuilder.Register("hello").MemberOf<IEnumerable<string>>();
            innerBuilder.Build(inner);

            var innerEnumerable = inner.Resolve<IEnumerable<string>>();
            var outerEnumerable = container.Resolve<IEnumerable<string>>();

            Assert.AreEqual(1, innerEnumerable.Count());
            Assert.IsTrue(innerEnumerable.Contains("hello"));
            Assert.IsFalse(outerEnumerable.Any());
        }

        [Test]
        public void CollectionRegistrationsSupportArrays()
        {
            var builder = new ContainerBuilder();

            builder.RegisterCollection<string>()
                .As<string[]>();

            var s1 = "hello";

            builder.Register(s1).MemberOf(typeof(string[]));

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
    }
}
