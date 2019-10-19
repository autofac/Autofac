using System;
using System.Threading.Tasks;

namespace Autofac.Test.Util
{
#if ASYNC_DISPOSE_AVAILABLE
    public class AsyncOnlyDisposeTracker : IAsyncDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsAsyncDisposed { get; set; }

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
