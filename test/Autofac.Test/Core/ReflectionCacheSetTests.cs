// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Util.Cache;

namespace Autofac.Test.Core;

public class ReflectionCacheSetTests
{
    [Fact]
    public void GettingCacheSubsequentlyReturnsSameInstance()
    {
        var set = new ReflectionCacheSet();

        var cache = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cache");

        var secondCache = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cache");

        Assert.Same(cache, secondCache);
    }

    [Fact]
    public void InvokingClearCallsClearOnAllCaches()
    {
        var set = new ReflectionCacheSet();

        var internalCache = set.Internal.IsGenericEnumerableInterface;
        internalCache[typeof(IEnumerable<>)] = true;

        var externalCache = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cache");
        externalCache[typeof(string)] = true;

        set.Clear();

        Assert.Empty(internalCache);
        Assert.Empty(externalCache);
    }

    [Fact]
    public void InvokingClearWithPredicateCallsClearOnAllCaches()
    {
        var set = new ReflectionCacheSet();

        var internalCache = set.Internal.IsGenericEnumerableInterface;
        internalCache[typeof(IEnumerable<>)] = true;
        internalCache[typeof(string)] = false;

        var externalCache = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cache");
        externalCache[typeof(string)] = true;

        set.Clear((member, assembly) => member == typeof(string));

        Assert.Collection(internalCache, item => Assert.Equal(typeof(IEnumerable<>), item.Key));
        Assert.Empty(externalCache);
    }

    [Fact]
    public void ChangingTypeBetweenCacheFetchesThrows()
    {
        var set = new ReflectionCacheSet();

        set.GetOrCreateCache<ReflectionCacheDictionary<PropertyInfo, bool>>("cache");

        Assert.Throws<InvalidOperationException>(() => set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cache"));
    }

    [Fact]
    public void OnContainerBuildClearCachesOnlyClearsRegistrationOnlyCaches()
    {
        var set = new ReflectionCacheSet();

        var cacheRegOnly = set.GetOrCreateCache("cacheRegistrationOnly", _ => new ReflectionCacheDictionary<Type, bool> { Usage = ReflectionCacheUsage.Registration });

        var cacheAllStages = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>("cacheAllStages");

        cacheRegOnly[typeof(string)] = true;
        cacheAllStages[typeof(string)] = true;

        set.OnContainerBuildClearCaches(clearRegistrationCaches: true);

        Assert.Empty(cacheRegOnly);
        Assert.NotEmpty(cacheAllStages);
    }
}
