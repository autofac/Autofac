// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.OwnedInstances;
using Autofac.Test.Util;

namespace Autofac.Test.Features.OwnedInstances;

public class OwnedTests
{
    [Fact]
    public void DisposingOwned_CallsDisposeOnLifetimeToken()
    {
        using var lifetime = new DisposeTracker();
        var owned = new Owned<string>("unused", lifetime);
        owned.Dispose();
        Assert.True(lifetime.IsDisposed);
    }

    [Fact]
    public async Task DisposingOwnedAsynchronously_CallsDisposeOnLifetimeTokenIfAsyncDisposableNotDeclared()
    {
        using var lifetime = new DisposeTracker();
        var owned = new Owned<string>("unused", lifetime);
        await owned.DisposeAsync();
        Assert.True(lifetime.IsDisposed);
    }

    [Fact]
    public async Task DisposingOwnedAsynchronously_CallsDisposeAsyncOnLifetimeTokenIfAsyncDisposableDeclared()
    {
        await using var lifetime = new AsyncDisposeTracker();
        var owned = new Owned<string>("unused", lifetime);
        await owned.DisposeAsync();
        Assert.True(lifetime.IsAsyncDisposed);
        Assert.False(lifetime.IsSyncDisposed);
    }

    [Fact]
    public void WhenInitialisedWithValue_ReturnsSameFromValueProperty()
    {
        var value = "Hello";
        using var instance = new DisposeTracker();
        using var owned = new Owned<string>(value, instance);
        Assert.Same(value, owned.Value);
    }
}
