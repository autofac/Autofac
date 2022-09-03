// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// A reflection cache dictionary, keyed on an <see cref="Assembly"/>.
/// </summary>
/// <typeparam name="TKey">The dictionary key.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class ReflectionCacheAssemblyDictionary<TKey, TValue>
    : ConcurrentDictionary<TKey, TValue>, IReflectionCache
    where TKey : Assembly
{
    /// <inheritdoc />
    public ReflectionCacheUsage Usage { get; set; } = ReflectionCacheUsage.All;

    /// <inheritdoc />
    public void Clear(ReflectionCacheClearPredicate? predicate)
    {
        if (predicate is null)
        {
            Clear();
            return;
        }

        foreach (var kvp in this)
        {
            if (predicate(kvp.Key, null))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }
}
