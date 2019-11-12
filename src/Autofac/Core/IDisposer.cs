﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
