// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Verifies that Autofac disposes owned <see cref="IDisposable"/> instances when
/// their lifetime scope is disposed.
/// </summary>
internal sealed class DisposableService : IDisposable
{
    public bool IsDisposed
    {
        get; private set;
    }

    public void Dispose()
    {
        IsDisposed = true;
    }
}
