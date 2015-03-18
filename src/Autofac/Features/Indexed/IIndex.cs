// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
// http://autofac.org
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

namespace Autofac.Features.Indexed
{
    /// <summary>
    /// Provides components by lookup operations via an index (key) type.
    /// </summary>
    /// <typeparam name="TKey">The type of the index.</typeparam>
    /// <typeparam name="TValue">The service provided by the indexed components.</typeparam>
    /// <example>
    /// Retrieving a value given a key:
    /// <code>
    /// IIndex&lt;AccountType, IRenderer&gt; accountRenderers = // ...
    /// var renderer = accountRenderers[AccountType.User];
    /// </code>
    /// </example>
    public interface IIndex<in TKey, TValue>
    {
        /// <summary>
        /// Get the value associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The value to retrieve.</param>
        /// <returns>The associated value.</returns>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Get the value associated with <paramref name="key"/> if any is available.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>True if a value associated with the key exists.</returns>
        bool TryGetValue(TKey key, out TValue value);
    }
}
