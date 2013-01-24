using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac.Integration.Mef;
using Autofac.Util;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class DisposalRegistrationTests
    {
        [Test]
        public void DefaultLifetimeForMefComponentsIsSingleton()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasDefaultCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        [Test]
        public void RespectsSharedCreationPolicy()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasSharedCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        [Test]
        public void AnyCreationPolicyDefaultsToShared()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasAnyCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            AssertDisposalTrackerIsSingleton(builder);
        }

        private static void AssertDisposalTrackerIsSingleton(ContainerBuilder builder)
        {
            var container = builder.Build();
            var instance1 = container.Resolve<DisposalTracker>();
            var instance2 = container.Resolve<DisposalTracker>();
            Assert.AreSame(instance1, instance2);
            Assert.IsFalse(instance1.IsDisposedPublic);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposedPublic);
        }

        [Test]
        public void RespectsNonSharedCreationPolicy()
        {
            var builder = new ContainerBuilder();
            var catalog = new TypeCatalog(typeof(HasNonSharedCreationPolicy));
            builder.RegisterComposablePartCatalog(catalog);
            var container = builder.Build();
            var instance1 = container.Resolve<DisposalTracker>();
            var instance2 = container.Resolve<DisposalTracker>();
            Assert.IsAssignableFrom<HasNonSharedCreationPolicy>(instance1);
            Assert.AreNotSame(instance1, instance2);
            Assert.IsFalse(instance1.IsDisposedPublic);
            Assert.IsFalse(instance2.IsDisposedPublic);
            container.Dispose();
            Assert.IsTrue(instance1.IsDisposedPublic);
            Assert.IsTrue(instance2.IsDisposedPublic);
        }

        public class DisposalTracker : Disposable
        {
            public bool IsDisposedPublic
            {
                get
                {
                    return this.IsDisposed;
                }
            }
        }

        [Export(typeof(DisposalTracker))]
        public class HasDefaultCreationPolicy : DisposalTracker
        {
        }

        [PartCreationPolicy(CreationPolicy.Any)]
        [Export(typeof(DisposalTracker))]
        public class HasAnyCreationPolicy : DisposalTracker
        {
        }

        [PartCreationPolicy(CreationPolicy.Shared)]
        [Export(typeof(DisposalTracker))]
        public class HasSharedCreationPolicy : DisposalTracker
        {
        }

        [PartCreationPolicy(CreationPolicy.NonShared)]
        [Export(typeof(DisposalTracker))]
        public class HasNonSharedCreationPolicy : DisposalTracker
        {
        }
    }
}
