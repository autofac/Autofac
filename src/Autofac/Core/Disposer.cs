// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int _purgeInterval = 5;
        private int _purgeCount;

        /// <summary>
        /// Contents all implement IDisposable or IAsyncDisposable.
        /// </summary>
        private List<object> _items = new List<object>();

        // Need to use a semaphore instead of a simple object to lock on, because
        // we need to synchronise an awaitable block.
        private readonly SemaphoreSlim _synchRoot = new SemaphoreSlim(1, 1);

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
                    _items.Reverse();
                    foreach (var item in _items)
                    {
                        var disposeItem = item is WeakReference wr ? wr.Target : item;

                        // If we are in synchronous dispose, and an object implements IDisposable,
                        // then use it.
                        if (disposeItem is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        else
                        {
                            // Type only implements IAsyncDisposable, which is not valid if there
                            // is a synchronous dispose being done.
                            throw new InvalidOperationException(string.Format(
                                DisposerResources.Culture,
                                DisposerResources.TypeOnlyImplementsIAsyncDisposable,
                                item.GetType().FullName));
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
                    _items.Reverse();
                    foreach (var item in _items)
                    {
                        var disposeItem = item is WeakReference wr ? wr.Target : item;

                        // If the item implements IAsyncDisposable we will call its DisposeAsync Method.
                        if (disposeItem is IAsyncDisposable asyncDisposable)
                        {
                            var vt = asyncDisposable.DisposeAsync();

                            // Don't await if it's already completed (this is a slight gain in performance of using ValueTask).
                            if (!vt.IsCompletedSuccessfully)
                            {
                                await vt.ConfigureAwait(false);
                            }
                        }
                        else if (disposeItem is IDisposable disposable)
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
        /// <param name="useWeakReference">if tue, the instance is wrapped in a WeakReference.</param>
        /// <remarks>
        /// If this Disposer is disposed of using a synchronous Dispose call, that call will throw an exception.
        /// </remarks>
        public void AddInstanceForAsyncDisposal(IAsyncDisposable instance, bool useWeakReference = true)
        {
            AddInternal(instance, useWeakReference);
        }

        /// <summary>
        /// Adds an object to the disposer. When the disposer is
        /// disposed, so will the object be.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="useWeakReference">if tue, the instance is wrapped in a WeakReference.</param>
        public void AddInstanceForDisposal(IDisposable instance, bool useWeakReference = true)
        {
            AddInternal(instance, useWeakReference);
        }

        private void AddInternal(object instance, bool useWeakReference = true)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Disposer), DisposerResources.CannotAddToDisposedDisposer);
            }

            if (useWeakReference)
            {
                instance = new WeakReference(instance);
            }

            _synchRoot.Wait();
            try
            {
                _items.Add(instance);

                if (_purgeCount++ >= _purgeInterval)
                {
                    PurgeGarbadgeCollectedItems();
                }
            }
            finally
            {
                _synchRoot.Release();
            }
        }

        private void PurgeGarbadgeCollectedItems()
        {
            foreach (var item in _items)
            {
                if (item is WeakReference { IsAlive: false })
                {
                    _items.Remove(item);
                }
            }

            _purgeCount = 0;
        }
    }
}
