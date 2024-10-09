// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Util;

public sealed class AsyncDisposeTracker : IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public event EventHandler<EventArgs> Disposing;

    public bool IsSyncDisposed { get; set; }

    public bool IsAsyncDisposed { get; set; }

    public AsyncDisposeTracker()
        : this(null)
    {
    }

    public AsyncDisposeTracker(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    public void Dispose()
    {
        IsSyncDisposed = true;

        Disposing?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        if (_semaphore != null)
        {
            await _semaphore.WaitAsync();
        }

        try
        {
            IsAsyncDisposed = true;

            Disposing?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _semaphore?.Release();
        }
    }
}
