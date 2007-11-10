using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Tests
{
    class DisposeTracker : IDisposable
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
