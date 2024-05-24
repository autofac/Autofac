// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Test.Util;

namespace Autofac.Test.Component.Activation;

public class ProvidedInstanceActivatorTests
{
    [Fact]
    public void NullIsNotAValidInstance()
    {
        Assert.Throws<ArgumentNullException>(() => new ProvidedInstanceActivator(null));
    }

    [Fact]
    public void WhenInitializedWithInstance_ThatInstanceIsReturnedFromActivateInstance()
    {
        var instance = new object();

        using var target = new ProvidedInstanceActivator(instance);

        using var container = Factory.CreateEmptyContainer();

        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var actual = invoker(container, Factory.NoParameters);

        Assert.Same(instance, actual);
    }

    [Fact]
    public void ActivatingAProvidedInstanceTwice_RaisesException()
    {
        var instance = new object();

        using var target = new ProvidedInstanceActivator(instance);

        using var container = Factory.CreateEmptyContainer();

        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        invoker(container, Factory.NoParameters);

        Assert.Throws<InvalidOperationException>(() =>
            invoker(container, Factory.NoParameters));
    }

    [Fact]
    [SuppressMessage("CA1849", "CA1849", Justification = "Handles specific test case of sync over async.")]
    public async Task SyncDisposeAsyncDisposable_DisposesAsExpected()
    {
        await using var asyncDisposable = new AsyncOnlyDisposeTracker();

        var target = new ProvidedInstanceActivator(asyncDisposable)
        {
            DisposeInstance = true,
        };

        target.Dispose();

        Assert.True(asyncDisposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task AsyncDisposeAsyncDisposable_DisposesAsExpected()
    {
        await using var asyncDisposable = new AsyncOnlyDisposeTracker();

        var target = new ProvidedInstanceActivator(asyncDisposable)
        {
            DisposeInstance = true,
        };

        await target.DisposeAsync();

        Assert.True(asyncDisposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task AsyncDisposeSyncDisposable_DisposesAsExpected()
    {
        using var asyncDisposable = new DisposeTracker();

        var target = new ProvidedInstanceActivator(asyncDisposable)
        {
            DisposeInstance = true,
        };

        await target.DisposeAsync();

        Assert.True(asyncDisposable.IsDisposed);
    }
}
