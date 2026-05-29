// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util;

/// <summary>
/// Base class for disposable objects.
/// </summary>
[SuppressMessage("S3881", "S3881", Justification = "Dispose is implemented correctly, analyzers just don't see it.")]
public class Disposable : IDisposable, IAsyncDisposable
{
    private const int DisposedFlag = 1;
    private int _isDisposed;

    /// <summary>
    /// Gets a value indicating whether the current instance has been disposed.
    /// </summary>
    protected bool IsDisposed
    {
        get
        {
            Interlocked.MemoryBarrier();
            return _isDisposed == DisposedFlag;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    [SuppressMessage("CA1063", "CA1063", Justification = "Dispose is implemented correctly, analyzers just don't see it.")]
    public void Dispose()
    {
        var wasDisposed = Interlocked.Exchange(ref _isDisposed, DisposedFlag);
        if (wasDisposed == DisposedFlag)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    [SuppressMessage("CA1816", "CA1816", Justification = "DisposeAsync should also call SuppressFinalize (see various .NET internal implementations).")]
    public ValueTask DisposeAsync()
    {
        // Still need to check if we've already disposed; can't do both.
        var wasDisposed = Interlocked.Exchange(ref _isDisposed, DisposedFlag);
        if (wasDisposed != DisposedFlag)
        {
            GC.SuppressFinalize(this);

            // Always true, but means we get the similar syntax as Dispose,
            // and separates the two overloads.
            return DisposeAsync(true);
        }

        return default;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    ///  Releases unmanaged and - optionally - managed resources, asynchronously.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    /// <returns>A task to await disposal.</returns>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        // Default implementation does a synchronous dispose.
        Dispose(disposing);

        return default;
    }
}
