using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;
using Autofac.Tests.Scenarios.Graph1;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class DisposerTests
    {
        [Test]
        public void OnDispose_DisposerDisposesContainedInstances()
        {
            var instance = new DisposeTracker();
            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.IsFalse(instance.IsDisposed);
            disposer.Dispose();
            Assert.IsTrue(instance.IsDisposed);
        }

        [Test]
        public void DisposerDisposesContainedInstances_InReverseOfOrderAdded()
        {
            DisposeTracker lastDisposed = null;

            var instance1 = new DisposeTracker();
            instance1.Disposing += (s, e) => lastDisposed = instance1;
            var instance2 = new DisposeTracker();
            instance2.Disposing += (s, e) => lastDisposed = instance2;

            var disposer = new Disposer();
            
            disposer.AddInstanceForDisposal(instance1);
            disposer.AddInstanceForDisposal(instance2);

            disposer.Dispose();

            Assert.AreSame(instance1, lastDisposed);
        }
    }
}
