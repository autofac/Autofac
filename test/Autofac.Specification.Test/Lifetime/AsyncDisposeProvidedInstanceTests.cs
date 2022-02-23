// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Specification.Test.Util;

namespace Autofac.Specification.Test.Lifetime;

public class AsyncDisposeProvidedInstanceTests
{
    [Fact]
    public async Task ResolvedProvidedInstances_DisposedWithLifetimeScope()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        builder.RegisterInstance(disposable);
        var container = builder.Build();
        container.Resolve<AsyncOnlyDisposeTracker>();

        await container.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task ResolvedProvidedInstances_DisposedWithLifetimeScope_OnlyDisposedOnce()
    {
        // Issue 383: Disposing a container should only dispose a provided instance one time.
        var builder = new ContainerBuilder();
        var count = 0;
        var disposable = new AsyncOnlyDisposeTracker();
        disposable.Disposing += (sender, e) => count++;
        builder.RegisterInstance(disposable);
        var container = builder.Build();
        container.Resolve<AsyncOnlyDisposeTracker>();

        await container.DisposeAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ResolvedProvidedInstances_DisposedWithNestedLifetimeScope()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));
        scope.Resolve<AsyncOnlyDisposeTracker>();

        await scope.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task ResolvedProvidedInstances_DisposedWithNestedLifetimeScope_OnlyDisposedOnce()
    {
        // Issue 383: Disposing a container should only dispose a provided instance one time.
        var builder = new ContainerBuilder();
        var count = 0;
        var disposable = new AsyncOnlyDisposeTracker();
        disposable.Disposing += (sender, e) => count++;
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));
        scope.Resolve<AsyncOnlyDisposeTracker>();

        await scope.DisposeAsync();
        Assert.Equal(1, count);

        await container.DisposeAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ResolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        builder.RegisterInstance(disposable).ExternallyOwned();
        var container = builder.Build();
        container.Resolve<AsyncOnlyDisposeTracker>();

        await container.DisposeAsync();

        Assert.False(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task ResolvedProvidedInstances_NotOwnedByNestedLifetimeScope_NeverDisposed()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable).ExternallyOwned());
        scope.Resolve<AsyncOnlyDisposeTracker>();

        await scope.DisposeAsync();
        Assert.False(disposable.IsAsyncDisposed);

        await container.DisposeAsync();
        Assert.False(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_DisposedWithLifetimeScope()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        builder.RegisterInstance(disposable);
        var container = builder.Build();

        await container.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_DisposedWithLifetimeScope_OnlyDisposedOnce()
    {
        var builder = new ContainerBuilder();
        var count = 0;
        var disposable = new AsyncOnlyDisposeTracker();
        disposable.Disposing += (sender, e) => count++;
        builder.RegisterInstance(disposable);
        var container = builder.Build();

        await container.DisposeAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_DisposedWithNestedLifetimeScope()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));

        await scope.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_ActualAsyncDisposable_CorrectlyDisposed()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker(completeAsync: true);
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));

        await scope.DisposeAsync();
        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_DisposedWithNestedLifetimeScope_OnlyDisposedOnce()
    {
        var builder = new ContainerBuilder();
        var count = 0;
        var disposable = new AsyncOnlyDisposeTracker();
        disposable.Disposing += (sender, e) => count++;
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));

        await scope.DisposeAsync();
        Assert.Equal(1, count);

        await container.DisposeAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        builder.RegisterInstance(disposable).ExternallyOwned();
        var container = builder.Build();

        await container.DisposeAsync();

        Assert.False(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task UnresolvedProvidedInstances_NotOwnedByNestedLifetimeScope_NeverDisposed()
    {
        var builder = new ContainerBuilder();
        var disposable = new AsyncOnlyDisposeTracker();
        var container = builder.Build();
        var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable).ExternallyOwned());

        await scope.DisposeAsync();
        Assert.False(disposable.IsAsyncDisposed);

        await container.DisposeAsync();
        Assert.False(disposable.IsAsyncDisposed);
    }
}
