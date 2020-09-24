// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core
{
    /// <summary>
    /// Provided on an object that will dispose of other objects when it is
    /// itself disposed.
    /// </summary>
    public interface IDisposer : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Adds an object to the disposer. When the disposer is
        /// disposed, so will the object be.
        /// </summary>
        /// <param name="instance">The instance.</param>
        void AddInstanceForDisposal(IDisposable instance);

        /// <summary>
        /// Adds an object to the disposer, where that object implements IAsyncDisposable. When the disposer is
        /// disposed, so will the object be.
        /// You should most likely implement IDisposable as well, and call <see cref="AddInstanceForDisposal(IDisposable)"/> instead of this method.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <remarks>
        /// If the provided object only implements IAsyncDisposable, and the <see cref="IDisposer"/> is disposed of using a synchronous Dispose call,
        /// that call will throw an exception when it attempts to dispose of the provided instance.
        /// </remarks>
        void AddInstanceForAsyncDisposal(IAsyncDisposable instance);
    }
}
