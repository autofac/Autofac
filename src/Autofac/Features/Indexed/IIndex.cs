// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
