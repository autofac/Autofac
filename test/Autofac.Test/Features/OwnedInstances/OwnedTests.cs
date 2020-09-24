// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Features.OwnedInstances
{
    public class OwnedTests
    {
        [Fact]
        public void DisposingOwned_CallsDisposeOnLifetimeToken()
        {
            var lifetime = new DisposeTracker();
            var owned = new Owned<string>("unused", lifetime);
            owned.Dispose();
            Assert.True(lifetime.IsDisposed);
        }

        [Fact]
        public async Task DisposingOwnedAsynchronously_CallsDisposeOnLifetimeTokenIfAsyncDisposableNotDeclared()
        {
            var lifetime = new DisposeTracker();
            var owned = new Owned<string>("unused", lifetime);
            await owned.DisposeAsync();
            Assert.True(lifetime.IsDisposed);
        }

        [Fact]
        public async Task DisposingOwnedAsynchronously_CallsDisposeAsyncOnLifetimeTokenIfAsyncDisposableDeclared()
        {
            var lifetime = new AsyncDisposeTracker();
            var owned = new Owned<string>("unused", lifetime);
            await owned.DisposeAsync();
            Assert.True(lifetime.IsAsyncDisposed);
            Assert.False(lifetime.IsSyncDisposed);
        }

        [Fact]
        public void WhenInitialisedWithValue_ReturnsSameFromValueProperty()
        {
            var value = "Hello";
            var owned = new Owned<string>(value, new DisposeTracker());
            Assert.Same(value, owned.Value);
        }
    }
}
