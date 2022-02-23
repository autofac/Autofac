// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

            Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}
