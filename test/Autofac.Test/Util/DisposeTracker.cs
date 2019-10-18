using System;

namespace Autofac.Test.Util
{
    public class DisposeTracker : IDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            this.IsDisposed = true;

            if (this.Disposing != null)
            {
                this.Disposing(this, EventArgs.Empty);
            }
        }
    }
}
