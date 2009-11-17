using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            cb.RegisterInstance(containerDisposeTracker).Named("tracker");
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
    }
}
