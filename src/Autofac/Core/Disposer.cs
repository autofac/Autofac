// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Autofac.Util;

namespace Autofac.Core;

/// <summary>
/// Maintains a set of objects to dispose, and disposes them in the reverse order
/// from which they were added when the Disposer is itself disposed.
/// </summary>
internal class Disposer : Disposable, IDisposer
{
    /// <summary>
    /// Contents all implement IDisposable or IAsyncDisposable. Also locked on when adding or removing items;
    /// items are disposed outside the lock, so an async dispose never holds it across an await.
    /// </summary>
    private readonly Stack<object> _items = new();

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

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            while (TryPop(out var item))
            {
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
                    // So we push this disposal onto the thread pool.
                    Task.Run(async () => await asyncDisposable.DisposeAsync().ConfigureAwait(false))
                        .ConfigureAwait(false)
                        .GetAwaiter().GetResult();
                }
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            while (TryPop(out var item))
            {
                // If the item implements IAsyncDisposable we will call its DisposeAsync Method.
                if (item is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (item is IDisposable disposable)
                {
                    // Call the standard Dispose.
                    disposable.Dispose();
                }
            }
        }
    }

    private void AddInternal(object instance)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        lock (_items)
        {
            // Checked under the lock: the disposed flag is set before disposal starts popping,
            // so an item can't be added once disposal may have emptied the stack.
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Disposer), DisposerResources.CannotAddToDisposedDisposer);
            }

            _items.Push(instance);
        }
    }

    private bool TryPop([NotNullWhen(true)] out object? item)
    {
        lock (_items)
        {
            if (_items.Count > 0)
            {
                item = _items.Pop();
                return true;
            }
        }

        item = null;
        return false;
    }
}
