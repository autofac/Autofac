using System;
using System.Threading.Tasks;

namespace Autofac.Test.Util
{
    public class AsyncOnlyDisposeTracker : IAsyncDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsAsyncDisposed { get; set; }

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
