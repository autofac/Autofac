// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Autofac.Util.Cache;

/// <summary>
/// An internal variant of <see cref="ReflectionCacheDictionary{TKey, TValue}"/>
/// that handles a tuple key. Instances of this class must only be held as
/// static readonly fields.
/// </summary>
/// <typeparam name="TKey">The member items in the tuple key.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal class ReflectionCacheTupleDictionary<TKey, TValue>
    : ConcurrentDictionary<(TKey, TKey), TValue>, IReflectionCacheStore
    where TKey : MemberInfo
{
    public bool UsedAtRegistrationOnly { get; set; }

    public void Clear(ReflectionCacheShouldClearPredicate predicate)
    {
        if (predicate is null)
        {
            Clear();
            return;
        }

        foreach (var kvp in this)
        {
            if (predicate(GetKeyAssembly(kvp.Key.Item1), kvp.Key.Item1) ||
                predicate(GetKeyAssembly(kvp.Key.Item2), kvp.Key.Item2))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }

    private static Assembly GetKeyAssembly(TKey key)
    {
        if (key is Type keyType)
        {
            return keyType.Assembly;
        }
        else if (key.DeclaringType is Type declaredType)
        {
            return declaredType.Assembly;
        }

        throw new InvalidOperationException("Impossible state");
    }
}
