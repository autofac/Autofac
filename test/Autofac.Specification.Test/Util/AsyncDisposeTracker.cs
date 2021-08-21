// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Autofac.Specification.Test.Util
{
    public class AsyncDisposeTracker : IAsyncDisposable
    {
        public event EventHandler<EventArgs> Disposing;

        public bool IsDisposed { get; set; }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            Disposing?.Invoke(this, EventArgs.Empty);

            return default;
        }
    }
}
