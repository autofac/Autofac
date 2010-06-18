using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Core;
using NUnit.Framework;
using Autofac.Features.OwnedInstances;

namespace Autofac.Tests.Features.OwnedInstances
{
    [TestFixture]
    public class OwnedInstanceRegistrationSourceTests
    {
        [Test]
        public void WhenTIsRegistered_OwnedTCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();
            var owned = c.Resolve<Owned<DisposeTracker>>();
            Assert.IsNotNull(owned.Value);
        }

        [Test]
        public void CallingDisposeOnGeneratedOwnedT_DisposesOwnedInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();

            var owned = c.Resolve<Owned<DisposeTracker>>();
            var dt = owned.Value;
            Assert.IsFalse(dt.IsDisposed);
            owned.Dispose();
            Assert.IsTrue(dt.IsDisposed);
        }

        [Test]
        public void CallingDisposeOnGeneratedOwnedT_DoesNotDisposeCurrentLifetimeScope()
        {
            var cb = new ContainerBuilder();
            var containerDisposeTracker = new DisposeTracker();
            cb.RegisterInstance(containerDisposeTracker).Named<DisposeTracker>("tracker");
            cb.RegisterType<DisposeTracker>();
            var c = cb.Build();

            var owned = c.Resolve<Owned<DisposeTracker>>();
            owned.Dispose();
            Assert.IsFalse(containerDisposeTracker.IsDisposed);
        }

        [Test]
        public void IfInnerTypeIsNotRegistered_OwnedTypeIsNotEither()
        {
            var c = new ContainerBuilder().Build();
            Assert.IsFalse(c.IsRegistered<Owned<Object>>());
        }

        [Test]
        public void ResolvingOwnedInstanceByName_ReturnsValueByName()
        {
            object o = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(o).Named<object>("o");
            var container = builder.Build();

            var owned = container.Resolve<Owned<object>>("o");

            Assert.AreSame(o, owned.Value);
        }

        public class ExposesScopeTag
        {
            readonly ILifetimeScope _myScope;

            public ExposesScopeTag(ILifetimeScope myScope)
            {
                _myScope = myScope;
            }

            public object Tag { get { return _myScope.Tag; } }
        }

        [Test]
        public void ScopesCreatedForOwnedInstances_AreTaggedWithTheServiceThatIsOwned()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<ExposesScopeTag>();
            var c = cb.Build();

            var est = c.Resolve<Owned<ExposesScopeTag>>();
            Assert.AreEqual(new TypedService(typeof(ExposesScopeTag)), est.Value.Tag);
        }

        public class ClassWithFactory
        {
            public delegate Owned<ClassWithFactory> OwnedFactory();
        }

        [Test]
        public void CanResolveAndUse_OwnedGeneratedFactory()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<ClassWithFactory>();
            cb.RegisterGeneratedFactory<ClassWithFactory.OwnedFactory>();
            var c = cb.Build();
            var factory = c.Resolve<ClassWithFactory.OwnedFactory>();
            bool isAccessed;
            using(var owner = factory())
            {
                Assert.IsInstanceOf<ClassWithFactory>(owner.Value);
                isAccessed = true;
            }
            Assert.IsTrue(isAccessed);
        }
    }
}
