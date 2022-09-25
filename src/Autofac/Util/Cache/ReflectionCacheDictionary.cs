// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// A reflection cache dictionary, keyed on a <see cref="MemberInfo"/>, such as <see cref="Type"/> or <see cref="MethodInfo"/>.
/// </summary>
/// <typeparam name="TKey">The dictionary key, derived from <see cref="MemberInfo"/>.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class ReflectionCacheDictionary<TKey, TValue>
    : ConcurrentDictionary<TKey, TValue>, IReflectionCache
    where TKey : MemberInfo
{
    /// <inheritdoc />
    public ReflectionCacheUsage Usage { get; set; } = ReflectionCacheUsage.All;

    /// <inheritdoc />
    public void Clear(ReflectionCacheClearPredicate predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

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
