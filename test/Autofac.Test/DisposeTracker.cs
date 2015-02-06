using System;

namespace Autofac.Test
{
    public class DisposeTracker : IDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;

            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
        }
    }
}
