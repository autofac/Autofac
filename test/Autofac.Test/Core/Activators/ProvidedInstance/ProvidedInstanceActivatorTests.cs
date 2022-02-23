// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Component.Activation
{
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

            var target = new ProvidedInstanceActivator(instance);

            var container = Factory.CreateEmptyContainer();

            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            var actual = invoker(container, Factory.NoParameters);

            Assert.Same(instance, actual);
        }

        [Fact]
        public void ActivatingAProvidedInstanceTwice_RaisesException()
        {
            var instance = new object();

            var target = new ProvidedInstanceActivator(instance);

            var container = Factory.CreateEmptyContainer();

            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            invoker(container, Factory.NoParameters);

            Assert.Throws<InvalidOperationException>(() =>
                invoker(container, Factory.NoParameters));
        }

        [Fact]
        public void SyncDisposeAsyncDisposable_DisposesAsExpected()
        {
            var asyncDisposable = new AsyncOnlyDisposeTracker();

            var target = new ProvidedInstanceActivator(asyncDisposable)
            {
                DisposeInstance = true
            };

            target.Dispose();

            Assert.True(asyncDisposable.IsAsyncDisposed);
        }

        [Fact]
        public async Task AsyncDisposeAsyncDisposable_DisposesAsExpected()
        {
            var asyncDisposable = new AsyncOnlyDisposeTracker();

            var target = new ProvidedInstanceActivator(asyncDisposable)
            {
                DisposeInstance = true
            };

            await target.DisposeAsync();

            Assert.True(asyncDisposable.IsAsyncDisposed);
        }

        [Fact]
        public async Task AsyncDisposeSyncDisposable_DisposesAsExpected()
        {
            var asyncDisposable = new DisposeTracker();

            var target = new ProvidedInstanceActivator(asyncDisposable)
            {
                DisposeInstance = true
            };

            await target.DisposeAsync();

            Assert.True(asyncDisposable.IsDisposed);
        }
    }
}
