using System;

namespace Autofac.Specification.Test.Util
{
    public class DisposeTracker : IDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
            Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}
