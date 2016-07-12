using System;

namespace Autofac.Net46.Test.Scenarios
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
