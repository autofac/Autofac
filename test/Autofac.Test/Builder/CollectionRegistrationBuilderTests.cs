using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Xunit;

namespace Autofac.Test.Builder
{
    public class CollectionRegistrationBuilderTests
    {
        [Fact]
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

            Assert.Equal(2, strings.Count());
            Assert.True(strings.Contains(s1));
            Assert.True(strings.Contains(s2));
        }

        [Fact]
        public void NewCollectionInNewContext()
        {
            var builder = new ContainerBuilder();
            var cname = "s";

            builder.RegisterCollection<string>(cname)
                .As<IEnumerable<string>>()
                .InstancePerLifetimeScope();

            var outer = builder.Build();

            var coll = outer.Resolve<IEnumerable<string>>();

            Assert.NotNull(coll);
            Assert.Same(coll, outer.Resolve<IEnumerable<string>>());

            var inner = outer.BeginLifetimeScope();

            Assert.NotSame(coll, inner.Resolve<IEnumerable<string>>());
        }

        [Fact]
        public void CollectionRegistrationsSupportArrays()
        {
            var builder = new ContainerBuilder();
            var cname = "s";

            builder.RegisterCollection<string>(cname).As<string[]>();

            var s1 = "hello";

            builder.RegisterInstance(s1).MemberOf(cname);

            var container = builder.Build();

            Assert.Equal(s1, container.Resolve<string[]>()[0]);
        }

        [Fact]
        public void CanRegisterCollectionWithoutGenericMethod()
        {
            var builder = new ContainerBuilder();
            var cname = "s";

            builder.RegisterCollection(cname, typeof(string))
                .As<string[]>();

            var container = builder.Build();

            Assert.True(container.IsRegistered<string[]>());
        }

        [Fact]
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

            Assert.Equal(1, coll1.Count());
            Assert.Equal(1, coll2.Count());
        }
    }
}
