﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
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
        private Stack<object> _items = new Stack<object>();

        // Need to use a semaphore instead of a simple object to lock on, because
        // we need to synchronise an awaitable block.
        private SemaphoreSlim _synchRoot = new SemaphoreSlim(1, 1);

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
            if (instance == null) throw new ArgumentNullException(nameof(instance));

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
