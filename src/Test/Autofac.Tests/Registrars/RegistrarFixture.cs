using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Registrars
{
    [TestFixture]
    public class RegistrarFixture
    {
        [Test]
        public void RegistrationMadeWhenPredicateTrue()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        public void RegistrationNotMadeWhenPredicateFalse()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => false);
            var container = cb.Build();
            Assert.IsFalse(container.IsRegistered<object>());
        }

        [Test]
        public void ContainerPassedToPredicate()
        {
            var cb = new ContainerBuilder();
            IContainer passed = null;
            cb.Register<object>().OnlyIf(c => { passed = c; return true; });
            var container = cb.Build();
            Assert.IsNotNull(passed);
            Assert.AreSame(passed, container);
        }

        [Test]
        public void OneFalsePredicatePreventsRegistration()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true).OnlyIf(c => false).OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsFalse(container.IsRegistered<object>());
        }

        [Test]
        public void AllTruePredicatesAllowsRegistration()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>().OnlyIf(c => true).OnlyIf(c => true).OnlyIf(c => true);
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        public void InContextSpecifiesContainerScope()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.Register<object>().InContext(contextName);
            var container = cb.Build();

            var ctx1 = container.CreateInnerContainer();
            ctx1.TagWith(contextName);

            var ctx2 = container.CreateInnerContainer();
            ctx2.TagWith(contextName);

            AssertIsContainerScoped<object>(ctx1, ctx2);
        }

        [Test]
        public void InContextDoesntOverrideFactoryScope()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.Register<object>().FactoryScoped().InContext(contextName);
            var container = cb.Build();

            var ctx1 = container.CreateInnerContainer();
            ctx1.TagWith(contextName);

            var ctx2 = container.CreateInnerContainer();
            ctx2.TagWith(contextName);

            AssertIsFactoryScoped<object>(ctx1, ctx2);
        }

        void AssertIsContainerScoped<TSvc>(IContainer ctx1, IContainer ctx2)
        {
            Assert.AreSame(ctx1.Resolve<TSvc>(), ctx1.Resolve<TSvc>());
            Assert.AreNotSame(ctx1.Resolve<TSvc>(), ctx2.Resolve<TSvc>());
        }

        void AssertIsFactoryScoped<TSvc>(IContainer ctx1, IContainer ctx2)
        {
            Assert.AreNotSame(ctx1.Resolve<TSvc>(), ctx1.Resolve<TSvc>());
            Assert.AreNotSame(ctx1.Resolve<TSvc>(), ctx2.Resolve<TSvc>());
        }

        void AssertIsSingletonScoped<TSvc>(IContainer ctx1, IContainer ctx2)
        {
            Assert.AreSame(ctx1.Resolve<TSvc>(), ctx1.Resolve<TSvc>());
            Assert.AreSame(ctx1.Resolve<TSvc>(), ctx2.Resolve<TSvc>());
        }
    }
}
