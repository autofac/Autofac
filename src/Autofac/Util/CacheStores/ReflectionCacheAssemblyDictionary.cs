// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// A concurrent dictionary that automatically registers with <see cref="ReflectionCache"/>.
/// </summary>
/// <typeparam name="TKey">The dictionary key, derived from <see cref="MemberInfo"/>.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ReflectionCacheAssemblyDictionary<TKey, TValue>
    : ConcurrentDictionary<TKey, TValue>, IReflectionCacheStore
    where TKey : Assembly
{
    public bool UsedAtRegistrationOnly { get; set; }

    /// <summary>
    /// Clear this cache.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter with.</param>
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
