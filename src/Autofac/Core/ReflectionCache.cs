// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Core;

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
public delegate bool ReflectionCacheClearPredicate(Assembly assembly, MemberInfo? member);

public class ReflectionCacheOptions
{
    public static ReflectionCacheOptions Default { get; } = new();

    public bool RetainAllCachesAfterContainerBuild { get; set; }
}

public sealed class ReflectionCache
{
    private static WeakReference<ReflectionCache>? _sharedCache;
    private static object _cacheAllocationLock = new();

    private readonly ConcurrentDictionary<string, IReflectionCacheStore> _caches = new();
    private readonly ReflectionCacheOptions _options;

    public static ReflectionCache Shared
    {
        get
        {
            if (!TryGetSharedCache(out var sharedCache))
            {
                lock (_cacheAllocationLock)
                {
                    // Check the cache again inside the lock, another thread may have updated it.
                    if (!TryGetSharedCache(out sharedCache))
                    {
                        sharedCache = new ReflectionCache();
                        _sharedCache = new WeakReference<ReflectionCache>(sharedCache);
                    }
                }
            }

            return sharedCache;
        }
    }

    private static bool TryGetSharedCache([NotNullWhen(true)] out ReflectionCache? sharedCache)
    {
        if (_sharedCache is null)
        {
            sharedCache = null;
            return false;
        }

        return _sharedCache.TryGetTarget(out sharedCache);
    }

    public ReflectionCache()
    {
        _options = ReflectionCacheOptions.Default;
    }

    public ReflectionCache(ReflectionCacheOptions cacheOptions)
    {
        _options = ReflectionCacheOptions.Default;
    }

    internal InternalReflectionCaches Internal { get; } = new();

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
    public void Clear(ReflectionCacheClearPredicate predicate)
    {
        foreach (var cache in GetAllCaches())
        {
            cache.Clear(predicate);
        }
    }

    protected IEnumerable<IReflectionCacheStore> GetAllCaches()
    {
        foreach (var internalItem in Internal.All)
        {
            yield return internalItem;
        }

        foreach (var externalItem in _caches)
        {
            yield return externalItem.Value;
        }
    }

    public void OnContainerBuild(IContainer container)
    {
        if (!_options.RetainAllCachesAfterContainerBuild)
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
}
