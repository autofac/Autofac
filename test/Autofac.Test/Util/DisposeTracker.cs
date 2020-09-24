// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Test.Util
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
