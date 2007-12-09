using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class FactoryRegistrationBuilderFixture
    {
        public class Named
        {
            public delegate Named Factory(string name);

            public string Name { get; set; }

            public Named(string name, object o)
            {
                Name = name;
            }
        }

        [Test]
        public void RegisterFactory()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>();
            cb.RegisterFactory<Named.Factory>((c, p) =>
                new Named(
                    p.Get<string>("name"),
                    c.Resolve<object>()));

            var container = cb.Build();
            Named.Factory factory = container.Resolve<Named.Factory>();
            Assert.IsNotNull(factory);

            string name = "Fred";
            var fred = factory.Invoke(name);
            Assert.IsNotNull(fred);
            Assert.AreEqual(name, fred.Name);
        }

        [Test]
        public void RegisterThroughFactory()
        {
            var cb = new ContainerBuilder();

            cb.Register<object>();
            cb.Register<Named>().ThroughFactory<Named.Factory>();

            var container = cb.Build();

            Named.Factory factory = container.Resolve<Named.Factory>();

            Assert.IsNotNull(factory);
            Assert.IsFalse(container.IsRegistered<Named>());

            string name = "Fred";
            var fred = factory.Invoke(name);
            Assert.IsNotNull(fred);
            Assert.AreEqual(name, fred.Name);
        }

    }
}
