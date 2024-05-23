// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Util;

public class DisposeTracker : IDisposable
{
    public event EventHandler<EventArgs> Disposing;

    public bool IsDisposed { get; set; }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
