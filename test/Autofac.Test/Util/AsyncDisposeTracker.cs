using System;
using System.Threading.Tasks;

namespace Autofac.Test.Util
{
#if NETCOREAPP3_0
    public class AsyncDisposeTracker : IDisposable, IAsyncDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsSyncDisposed { get; set; }

        public bool IsAsyncDisposed { get; set; }

        public void Dispose()
        {
            this.IsSyncDisposed = true;

            if (this.Disposing != null)
            {
                this.Disposing(this, EventArgs.Empty);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Delay(1);

            IsAsyncDisposed = true;

            if (this.Disposing != null)
            {
                this.Disposing(this, EventArgs.Empty);
            }
        }
    }
#endif
}
