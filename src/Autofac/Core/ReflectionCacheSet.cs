// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Core;

/// <summary>
/// Defines the set of all caches of reflection-related data.
/// Access the shared instance using <see cref="Shared"/>.
/// </summary>
public sealed class ReflectionCacheSet
{
    private static readonly object CacheAllocationLock = new();

    private static WeakReference<ReflectionCacheSet>? _sharedSet;

    private readonly ConcurrentDictionary<string, IReflectionCache> _caches = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionCacheSet"/> class.
    /// </summary>
    public ReflectionCacheSet()
    {
        Internal = new InternalReflectionCaches(this);
    }

    /// <summary>
    /// Gets the shared <see cref="ReflectionCacheSet"/>.
    /// </summary>
    /// <remarks>
    /// Avoid storing the value of this property, and access
    /// <see cref="GetOrCreateCache(string)"/> directly instead,
    /// to ensure caches can be freed correctly.
    /// </remarks>
    public static ReflectionCacheSet Shared
    {
        get
        {
            if (!TryGetSharedCache(out var sharedCache))
            {
                lock (CacheAllocationLock)
                {
                    // Check the cache again inside the lock, another thread may have updated it.
                    if (!TryGetSharedCache(out sharedCache))
                    {
                        sharedCache = new ReflectionCacheSet();
                        _sharedSet = new WeakReference<ReflectionCacheSet>(sharedCache);
                    }
                }
            }

            return sharedCache;
        }
    }

    /// <summary>
    /// Gets the instance of the known Internal caches defined in <see cref="InternalReflectionCaches"/>.
    /// </summary>
    internal InternalReflectionCaches Internal { get; }

    /// <summary>
    /// Get a typed cache store with a given name, that is held in this instance. An instance will be created if it does not already exist.
    /// </summary>
    /// <typeparam name="TCacheStore">
    /// The type of cache store; must implement <see cref="IReflectionCache"/>. See <see cref="ReflectionCacheDictionary{TKey, TValue}"/> for a typical example.
    /// </typeparam>
    /// <param name="cacheName">The unique name of the cache.</param>
    /// <returns>An instance of <typeparamref name="TCacheStore"/>.</returns>
    public TCacheStore GetOrCreateCache<TCacheStore>(string cacheName)
        where TCacheStore : IReflectionCache, new()
        => GetOrCreateCache(cacheName, CacheFactory<TCacheStore>.Factory);

    /// <summary>
    /// Get a typed cache store with a given name, that is held in this instance. An instance will be created if it does not already exist.
    /// </summary>
    /// <typeparam name="TCacheStore">
    /// The type of cache store; must implement <see cref="IReflectionCache"/>. See <see cref="ReflectionCacheDictionary{TKey, TValue}"/> for a typical example.
    /// </typeparam>
    /// <param name="cacheName">The unique name of the cache.</param>
    /// <param name="cacheFactory">A custom factory for the cache store.</param>
    /// <returns>An instance of <typeparamref name="TCacheStore"/>.</returns>
    public TCacheStore GetOrCreateCache<TCacheStore>(string cacheName, Func<string, TCacheStore> cacheFactory)
        where TCacheStore : IReflectionCache
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
            throw new InvalidOperationException(ReflectionCacheSetResources.CacheRetrievalTypeChange);
        }
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
    /// Invoked when the container is built, to allow the cache to apply clearing behavior.
    /// </summary>
    /// <param name="clearRegistrationCaches">True if we should clear caches marked only for registration.</param>
    internal void OnContainerBuildClearCaches(bool clearRegistrationCaches)
    {
        if (clearRegistrationCaches)
        {
            // Default behavior on container build is to clear any caches marked
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

    private static bool TryGetSharedCache([NotNullWhen(true)] out ReflectionCacheSet? sharedCache)
    {
        if (_sharedSet is null)
        {
            sharedCache = null;
            return false;
        }

        return _sharedSet.TryGetTarget(out sharedCache);
    }

    private IEnumerable<IReflectionCache> GetAllCaches()
    {
        foreach (var externalItem in _caches)
        {
            yield return externalItem.Value;
        }
    }

    private static class CacheFactory<TCacheStore>
        where TCacheStore : IReflectionCache, new()
    {
        public static Func<string, TCacheStore> Factory { get; } = static (k) => new TCacheStore();
    }
}
