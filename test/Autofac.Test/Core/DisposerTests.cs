// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Core
{
    public class DisposerTests
    {
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
        public void CannotAddObjectsToDisposerAfterSyncDispose()
        {
            var instance = new DisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsDisposed);
            Assert.False(instance.IsDisposed);
            disposer.Dispose();
            Assert.True(instance.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() =>
            {
                disposer.AddInstanceForDisposal(instance);
            });
        }

        [Fact]
        public async ValueTask DisposerDisposesOfObjectsAsyncIfIAsyncDisposableDeclared()
        {
            var instance = new AsyncDisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsSyncDisposed);
            Assert.False(instance.IsAsyncDisposed);
            var result = disposer.DisposeAsync();
            Assert.False(instance.IsSyncDisposed);

            // Dispose is happening async, so this won't be true yet.
            Assert.False(instance.IsAsyncDisposed);

            // Now we wait.
            await result;

            Assert.False(instance.IsSyncDisposed);
            Assert.True(instance.IsAsyncDisposed);
        }

        [Fact]
        public async ValueTask DisposerDisposesOfObjectsSyncIfIDisposableOnly()
        {
            var instance = new DisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsDisposed);
            await disposer.DisposeAsync();
            Assert.True(instance.IsDisposed);
        }

        [Fact]
        public void DisposerDisposesOfObjectsSyncIfIAsyncDisposableDeclaredButSyncDisposeCalled()
        {
            var instance = new AsyncDisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsSyncDisposed);
            Assert.False(instance.IsAsyncDisposed);
            disposer.Dispose();
            Assert.True(instance.IsSyncDisposed);
            Assert.False(instance.IsAsyncDisposed);
        }

        [Fact]
        public async ValueTask CannotAddObjectsToDisposerAfterAsyncDispose()
        {
            var instance = new AsyncDisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForDisposal(instance);
            Assert.False(instance.IsSyncDisposed);
            Assert.False(instance.IsAsyncDisposed);
            await disposer.DisposeAsync();
            Assert.False(instance.IsSyncDisposed);
            Assert.True(instance.IsAsyncDisposed);

            Assert.Throws<ObjectDisposedException>(() =>
            {
                disposer.AddInstanceForDisposal(instance);
            });
        }

        [Fact]
        public void SyncDisposalOnObjectWithNoIDisposableThrows()
        {
            var instance = new AsyncOnlyDisposeTracker();

            var disposer = new Disposer();
            disposer.AddInstanceForAsyncDisposal(instance);

            Assert.Throws<InvalidOperationException>(() =>
            {
                disposer.Dispose();
            });
        }

        [Fact]
        public async ValueTask DisposerAsyncDisposesContainedInstances_InReverseOfOrderAdded()
        {
            var disposeOrder = new List<object>();

            var asyncInstance1 = new AsyncDisposeTracker();
            asyncInstance1.Disposing += (s, e) => disposeOrder.Add(asyncInstance1);
            var asyncOnlyInstance2 = new AsyncOnlyDisposeTracker();
            asyncOnlyInstance2.Disposing += (s, e) => disposeOrder.Add(asyncOnlyInstance2);
            var syncInstance3 = new DisposeTracker();
            syncInstance3.Disposing += (s, e) => disposeOrder.Add(syncInstance3);
            var syncInstance4 = new DisposeTracker();
            syncInstance4.Disposing += (s, e) => disposeOrder.Add(syncInstance4);

            var disposer = new Disposer();

            disposer.AddInstanceForDisposal(asyncInstance1);
            disposer.AddInstanceForDisposal(syncInstance3);
            disposer.AddInstanceForDisposal(syncInstance4);
            disposer.AddInstanceForAsyncDisposal(asyncOnlyInstance2);

            await disposer.DisposeAsync();

            Assert.Collection(
                disposeOrder,
                o1 => Assert.Same(asyncOnlyInstance2, o1),
                o2 => Assert.Same(syncInstance4, o2),
                o3 => Assert.Same(syncInstance3, o3),
                o4 => Assert.Same(asyncInstance1, o4));
        }
    }
}
