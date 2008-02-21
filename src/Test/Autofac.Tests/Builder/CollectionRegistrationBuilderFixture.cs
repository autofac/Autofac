using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
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
    }
}
