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
    /// <param name="doNotAutoRegister">If set, the dictionary will not auto-register with <see cref="ReflectionCache"/>.</param>
    public ReflectionCacheDictionary(bool doNotAutoRegister = false)
    {
        if (!doNotAutoRegister)
        {
            ReflectionCache.Register(CacheClear);
        }
    }

    /// <summary>
    /// Clear this cache.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter with.</param>
    public void CacheClear(ReflectionCacheShouldClearPredicate? predicate)
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
