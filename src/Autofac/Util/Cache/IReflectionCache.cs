// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util.Cache;

public interface IReflectionCache
{
    void Clear();

    void Clear(ReflectionCacheShouldClearPredicate predicate);

    TCacheDictionary GetOrCreateCache<TCacheDictionary>(string cacheName)
        where TCacheDictionary : IReflectionCacheStore, new();
    TCacheDictionary GetOrCreateCache<TCacheDictionary>(string cacheName, Func<string, IReflectionCacheStore> cacheFactory)
        where TCacheDictionary : IReflectionCacheStore;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    void OnContainerBuild(IContainer container);

    internal InternalReflectionCaches Internal { get; }
}
