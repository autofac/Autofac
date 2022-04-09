// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Autofac.Util;

/// <summary>
/// An internal variant of <see cref="ReflectionCacheDictionary{TKey, TValue}"/>
/// that handles a tuple key. Instances of this class must only be held as
/// static readonly fields.
/// </summary>
/// <typeparam name="TKey">The member items in the tuple key.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal class ReflectionCacheTupleDictionary<TKey, TValue> : ConcurrentDictionary<(TKey, TKey), TValue>
    where TKey : MemberInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionCacheTupleDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ReflectionCacheTupleDictionary()
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
            if (predicate(kvp.Key.Item1) || predicate(kvp.Key.Item2))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }
}
