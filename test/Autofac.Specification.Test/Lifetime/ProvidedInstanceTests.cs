// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class ProvidedInstanceTests
    {
        [Fact]
        public void ResolvedProvidedInstances_DisposedWithLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Resolve<DisposeTracker>();

            container.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void ResolvedProvidedInstances_DisposedWithLifetimeScope_OnlyDisposedOnce()
        {
            // Issue 383: Disposing a container should only dispose a provided instance one time.
            var builder = new ContainerBuilder();
            var count = 0;
            var disposable = new DisposeTracker();
            disposable.Disposing += (sender, e) => count++;
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Resolve<DisposeTracker>();

            container.Dispose();

            Assert.Equal(1, count);
        }

        [Fact]
        public void ResolvedProvidedInstances_DisposedWithNestedLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));
            scope.Resolve<DisposeTracker>();

            scope.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void ResolvedProvidedInstances_DisposedWithNestedLifetimeScope_OnlyDisposedOnce()
        {
            // Issue 383: Disposing a container should only dispose a provided instance one time.
            var builder = new ContainerBuilder();
            var count = 0;
            var disposable = new DisposeTracker();
            disposable.Disposing += (sender, e) => count++;
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));
            scope.Resolve<DisposeTracker>();

            scope.Dispose();
            Assert.Equal(1, count);

            container.Dispose();
            Assert.Equal(1, count);
        }

        [Fact]
        public void ResolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable).ExternallyOwned();
            var container = builder.Build();
            container.Resolve<DisposeTracker>();

            container.Dispose();

            Assert.False(disposable.IsDisposed);
        }

        [Fact]
        public void ResolvedProvidedInstances_NotOwnedByNestedLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable).ExternallyOwned());
            scope.Resolve<DisposeTracker>();

            scope.Dispose();
            Assert.False(disposable.IsDisposed);

            container.Dispose();
            Assert.False(disposable.IsDisposed);
        }

        [Fact]
        public void UnresolvedProvidedInstances_DisposedWithLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable);
            var container = builder.Build();

            container.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void UnresolvedProvidedInstances_DisposedWithLifetimeScope_OnlyDisposedOnce()
        {
            var builder = new ContainerBuilder();
            var count = 0;
            var disposable = new DisposeTracker();
            disposable.Disposing += (sender, e) => count++;
            builder.RegisterInstance(disposable);
            var container = builder.Build();

            container.Dispose();

            Assert.Equal(1, count);
        }

        [Fact]
        public void UnresolvedProvidedInstances_DisposedWithNestedLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));

            scope.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void UnresolvedProvidedInstances_DisposedWithNestedLifetimeScope_OnlyDisposedOnce()
        {
            var builder = new ContainerBuilder();
            var count = 0;
            var disposable = new DisposeTracker();
            disposable.Disposing += (sender, e) => count++;
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable));

            scope.Dispose();
            Assert.Equal(1, count);

            container.Dispose();
            Assert.Equal(1, count);
        }

        [Fact]
        public void UnresolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable).ExternallyOwned();
            var container = builder.Build();

            container.Dispose();

            Assert.False(disposable.IsDisposed);
        }

        [Fact]
        public void UnresolvedProvidedInstances_NotOwnedByNestedLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(disposable).ExternallyOwned());

            scope.Dispose();
            Assert.False(disposable.IsDisposed);

            container.Dispose();
            Assert.False(disposable.IsDisposed);
        }
    }
}
