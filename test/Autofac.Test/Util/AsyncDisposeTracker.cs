using System;
using System.Threading.Tasks;

namespace Autofac.Test.Util
{
    public class AsyncDisposeTracker : IDisposable, IAsyncDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsSyncDisposed { get; set; }

        public bool IsAsyncDisposed { get; set; }

        public void Dispose()
        {
            IsSyncDisposed = true;

            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Delay(1);

            IsAsyncDisposed = true;

            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }
    }
}
