// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Core;

/// <summary>
/// Defines a cache of reflection-related data. Access the shared instance using <see cref="ReflectionCache.Shared"/>.
/// </summary>
public sealed class ReflectionCache
{
    private static WeakReference<ReflectionCache>? _sharedCache;
    private static object _cacheAllocationLock = new();

    private readonly ConcurrentDictionary<string, IReflectionCacheStore> _caches = new();

    /// <summary>
    /// Gets the shared <see cref="ReflectionCache"/>.
    /// </summary>
    /// <remarks>
    /// Avoid storing the value of this property, and access
    /// <see cref="GetOrCreateCache(string)"/> directly instead,
    /// to ensure caches can be freed correctly.
    /// </remarks>
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

    /// <summary>
    /// Gets the instance of the known Internal caches defined in <see cref="InternalReflectionCaches"/>.
    /// </summary>
    internal InternalReflectionCaches Internal { get; } = new();

    /// <summary>
    /// Get a typed cache store with a given name, that is held in this instance. An instance will be created if it does not already exist.
    /// </summary>
    /// <typeparam name="TCacheStore">
    /// The type of cache store; must implement <see cref="IReflectionCacheStore"/>. See <see cref="ReflectionCacheDictionary{TKey, TValue}"/> for a typical example.
    /// </typeparam>
    /// <param name="cacheName">The unique name of the cache.</param>
    /// <returns>An instance of <typeparamref name="TCacheStore"/>.</returns>
    public TCacheStore GetOrCreateCache<TCacheStore>(string cacheName)
        where TCacheStore : IReflectionCacheStore, new()
        => GetOrCreateCache<TCacheStore>(cacheName, CacheFactory<TCacheStore>.Factory);

    /// <summary>
    /// Get a typed cache store with a given name, that is held in this instance. An instance will be created if it does not already exist.
    /// </summary>
    /// <typeparam name="TCacheStore">
    /// The type of cache store; must implement <see cref="IReflectionCacheStore"/>. See <see cref="ReflectionCacheDictionary{TKey, TValue}"/> for a typical example.
    /// </typeparam>
    /// <param name="cacheName">The unique name of the cache.</param>
    /// <param name="cacheFactory">A custom factory for the cache store.</param>
    /// <returns>An instance of <typeparamref name="TCacheStore"/>.</returns>
    public TCacheStore GetOrCreateCache<TCacheStore>(string cacheName, Func<string, TCacheStore> cacheFactory)
        where TCacheStore : IReflectionCacheStore
    {
        // This path is present for external code that wishes to store items in the reflection cache.
        try
        {
#if NETSTANDARD2_0
            return (TCacheStore)_caches.GetOrAdd(cacheName, name => cacheFactory(name));
#else
            // A bit of get/add indirection here we can only do after NS2.0 to get the right type of object to store in the dictionary,
            // without allocating a closure over cacheFactory, while still allowing cacheFactory to be strongly typed.
            return (TCacheStore)_caches.GetOrAdd(cacheName, static (name, factory) => factory(name), cacheFactory);
#endif
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException("Attempted to retrieve a previously stored cache with a different type than originally stored.");
        }
    }

    private static class CacheFactory<TCacheStore>
        where TCacheStore : IReflectionCacheStore, new()
    {
        public static Func<string, TCacheStore> Factory { get; } = static (k) => new TCacheStore();
    }

    /// <summary>
    /// Clear the internal reflection cache. Only call this method if you are
    /// dynamically unloading types from the process; calling this method
    /// otherwise will only slow down Autofac during normal operation.
    /// </summary>
    /// <remarks>
    /// This method is non-deterministic; there is no guarantee that the cache
    /// will be empty by the time the method exits, or that all items matched by
    /// the provided predicate have been removed.
    /// </remarks>
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

    /// <summary>
    /// Invoked when the container is built, to allow the cache to apply clearing behaviour.
    /// </summary>
    /// <param name="clearRegistrationCaches">True if we should clear caches marked only for registration.</param>
    internal void OnContainerBuild(bool clearRegistrationCaches)
    {
        if (clearRegistrationCaches)
        {
            // Default behaviour on container build is to clear any caches marked
            // as only being used during registration.
            foreach (var cache in GetAllCaches())
            {
                // Cache is not used at resolution stage, so clear it.
                if (!cache.Usage.HasFlag(ReflectionCacheUsage.Resolution))
                {
                    cache.Clear();
                }
            }
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

    private IEnumerable<IReflectionCacheStore> GetAllCaches()
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
}
