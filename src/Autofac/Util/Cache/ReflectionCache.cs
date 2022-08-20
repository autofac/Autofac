// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Util.Cache;

/// <summary>
/// Delegate for predicates that can choose whether to remove a member from the
/// reflection cache.
/// </summary>
/// <param name="member">
/// The member information (will be an instance of a more-derived type).
/// </param>
/// <returns>
/// True to remove the member from the cache, false to leave it.
/// </returns>
public delegate bool ReflectionCacheShouldClearPredicate(Assembly assembly, MemberInfo? member);

public sealed class DefaultReflectionCache : ReflectionCache
{
    private static IReflectionCache? _sharedCache;

    public static IReflectionCache Shared => _sharedCache ??= new DefaultReflectionCache();

    public override void OnContainerBuild(IContainer container)
    {
        // Default behaviour on container build is to clear any caches marked
        // as only being used during registration.
        foreach (var cache in GetAllCaches())
        {
            if (cache.UsedAtRegistrationOnly)
            {
                cache.Clear();
            }
        }
    }
}

public abstract class ReflectionCache : IReflectionCache
{
    private readonly ConcurrentDictionary<string, IReflectionCacheStore> _caches = new();
    private readonly InternalReflectionCaches _internal = new();

    InternalReflectionCaches IReflectionCache.Internal => _internal;

    public TCacheDictionary GetOrCreateCache<TCacheDictionary>(string cacheName)
        where TCacheDictionary : IReflectionCacheStore, new()
        => GetOrCreateCache<TCacheDictionary>(cacheName, CacheFactory<TCacheDictionary>.Factory);

    public TCacheDictionary GetOrCreateCache<TCacheDictionary>(string cacheName, Func<string, IReflectionCacheStore> cacheFactory)
        where TCacheDictionary : IReflectionCacheStore
    {
        // This path is present for external code that wishes to store items in the reflection cache.
        try
        {
            return (TCacheDictionary)_caches.GetOrAdd(cacheName, cacheFactory);
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException("Attempted to retrieve a previously stored cache with a different type than originally stored.");
        }
    }

    private static class CacheFactory<TCacheDictionary>
        where TCacheDictionary : IReflectionCacheStore, new()
    {
        public static Func<string, IReflectionCacheStore> Factory { get; } = (k) => new TCacheDictionary();
    }

    public void Clear()
    {
        foreach (var cache in GetAllCaches())
        {
            cache.Clear();
        }
    }

    /// <summary>
    /// Clear the internal reflection cache. Only call this method if you are
    /// dynamically unloading types from the process; calling this method
    /// otherwise will only slow down Autofac during normal operation.
    /// </summary>
    /// <param name="predicate">
    /// A predicate that can be used to filter which items get removed from the
    /// cache.
    /// </param>
    /// <remarks>
    /// This method is non-deterministic; there is no guarantee that the cache
    /// will be empty by the time the method exits, or that all items matched by
    /// the provided predicate have been removed.
    /// </remarks>
    public void Clear(ReflectionCacheShouldClearPredicate predicate)
    {
        foreach (var cache in GetAllCaches())
        {
            cache.Clear(predicate);
        }
    }

    protected IEnumerable<IReflectionCacheStore> GetAllCaches()
    {
        foreach (var internalItem in _internal.All)
        {
            yield return internalItem;
        }

        foreach (var externalItem in _caches)
        {
            yield return externalItem.Value;
        }
    }

    public abstract void OnContainerBuild(IContainer container);
}
