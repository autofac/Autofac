// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Autofac.Util;

/// <summary>
/// A concurrent dictionary that automatically registers with <see cref="ReflectionCache"/>. Instances
/// of this class must only be held as static readonly fields.
/// </summary>
/// <typeparam name="TKey">The dictionary key, derived from <see cref="MemberInfo"/>.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal class ReflectionCacheDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    where TKey : MemberInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionCacheDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ReflectionCacheDictionary()
    {
        ReflectionCache.Register(CacheClear);
    }

    private void CacheClear(ReflectionCacheShouldClearPredicate? predicate)
    {
        if (predicate is null)
        {
            Clear();
            return;
        }

        foreach (var kvp in this)
        {
            if (predicate(kvp.Key))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }
}
