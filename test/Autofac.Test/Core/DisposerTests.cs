using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Autofac.Test.Scenarios.Graph1;
using Xunit;

namespace Autofac.Test.Core
{
    public class DisposerTests
    {
        [Fact]
        public void OnDispose_DisposerDisposesContainedInstances()
        {
            var instance = new DisposeTracker();
            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsDisposed);
            disposer.Dispose();
            Assert.True(instance.IsDisposed);
        }

        [Fact]
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

            Assert.Same(instance1, lastDisposed);
        }
    }
}
