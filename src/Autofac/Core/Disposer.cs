// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Maintains a set of objects to dispose, and disposes them in the reverse order
    /// from which they were added when the Disposer is itself disposed.
    /// </summary>
    internal class Disposer : Disposable, IDisposer
    {
        /// <summary>
        /// Contents all implement IDisposable or IAsyncDisposable.
        /// </summary>
        private Stack<object> _items = new();

        // Need to use a semaphore instead of a simple object to lock on, because
        // we need to synchronise an awaitable block.
        private readonly SemaphoreSlim _synchRoot = new(1, 1);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _synchRoot.Wait();
                try
                {
                    while (_items.Count > 0)
                    {
                        var item = _items.Pop();

                        // If we are in synchronous dispose, and an object implements IDisposable,
                        // then use it.
                        if (item is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        else if (item is IAsyncDisposable asyncDisposable)
                        {
                            Trace.TraceWarning(DisposerResources.TypeOnlyImplementsIAsyncDisposable, item.GetType().FullName);

                            // Type only implements IAsyncDisposable. We will need to do sync-over-async.
                            // We want to ensure we lose all context here, because if we don't we can deadlock.
                            // So we push this disposal onto the threadpool.
                            Task.Run(async () => await asyncDisposable.DisposeAsync().ConfigureAwait(false))
                                .ConfigureAwait(false)
                                .GetAwaiter().GetResult();
                        }
                    }

                    // Explicitly clear to null, which is slightly cheating the compiler,
                    // but I know this method will never enter again (because it derives from the Disposable, which prevents it).
                    _items = null!;
                }
                finally
                {
                    _synchRoot.Release();

                    // We don't need the semaphore any more.
                    _synchRoot.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                // Acquire our semaphore.
                await _synchRoot.WaitAsync().ConfigureAwait(false);
                try
                {
                    while (_items.Count > 0)
                    {
                        var item = _items.Pop();

                        // If the item implements IAsyncDisposable we will call its DisposeAsync Method.
                        if (item is IAsyncDisposable asyncDisposable)
                        {
                            var vt = asyncDisposable.DisposeAsync();

                            // Don't await if it's already completed (this is a slight gain in performance of using ValueTask).
                            if (!vt.IsCompletedSuccessfully)
                            {
                                await vt.ConfigureAwait(false);
                            }
                        }
                        else if (item is IDisposable disposable)
                        {
                            // Call the standard Dispose.
                            disposable.Dispose();
                        }
                    }

                    // Explicitly clear to null, which is slightly cheating the compiler,
                    // but I know this method will never enter again (because it derives from the Disposable, which prevents it).
                    _items = null!;
                }
                finally
                {
                    _synchRoot.Release();

                    // We don't need the semaphore any more.
                    _synchRoot.Dispose();
                }
            }
        }

        /// <summary>
        /// Adds an object to the disposer, where that object only implements IAsyncDisposable. When the disposer is
        /// disposed, so will the object be.
        /// This is not typically recommended, and you should implement IDisposable as well.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <remarks>
        /// If this Disposer is disposed of using a synchronous Dispose call, that call will throw an exception.
        /// </remarks>
        public void AddInstanceForAsyncDisposal(IAsyncDisposable instance)
        {
            AddInternal(instance);
        }

        /// <summary>
        /// Adds an object to the disposer. When the disposer is
        /// disposed, so will the object be.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void AddInstanceForDisposal(IDisposable instance)
        {
            AddInternal(instance);
        }

        private void AddInternal(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Disposer), DisposerResources.CannotAddToDisposedDisposer);
            }

            _synchRoot.Wait();
            try
            {
                _items.Push(instance);
            }
            finally
            {
                _synchRoot.Release();
            }
        }
    }
}
