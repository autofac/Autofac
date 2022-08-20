// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Autofac.Util.Cache;

/// <summary>
/// A concurrent dictionary that automatically registers with <see cref="ReflectionCache"/>.
/// </summary>
/// <typeparam name="TKey">The dictionary key, derived from <see cref="MemberInfo"/>.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal class ReflectionCacheDictionary<TKey, TValue>
    : ConcurrentDictionary<TKey, TValue>, IReflectionCacheStore
    where TKey : MemberInfo
{
    public bool UsedAtRegistrationOnly { get; set; }

    /// <summary>
    /// Clear this cache.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter with.</param>
    public void Clear(ReflectionCacheShouldClearPredicate predicate)
    {
        foreach (var kvp in this)
        {
            if (kvp.Key is Type keyType)
            {
                if (predicate(keyType.Assembly, keyType))
                {
                    TryRemove(kvp.Key, out _);
                }
            }
            else if (kvp.Key.DeclaringType is Type declaredType)
            {
                if (predicate(declaredType.Assembly, declaredType))
                {
                    TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
