// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

            Disposing?.Invoke(this, EventArgs.Empty);
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Delay(1);

            IsAsyncDisposed = true;

            Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}
