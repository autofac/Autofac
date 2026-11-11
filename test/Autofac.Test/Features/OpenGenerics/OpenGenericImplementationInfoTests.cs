// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Features.OpenGenerics;

namespace Autofac.Test.Features.OpenGenerics;

public class OpenGenericImplementationInfoTests
{
    [Fact]
    public void ForCachesTheInfoInTheSharedInternalCache()
    {
        var info = OpenGenericImplementationInfo.For(typeof(Cached<>));

        Assert.Same(info, ReflectionCacheSet.Shared.Internal.OpenGenericImplementationInfoByType[typeof(Cached<>)]);
        Assert.Same(info, OpenGenericImplementationInfo.For(typeof(Cached<>)));
    }

    [Fact]
    public void CacheIsEmptiedByReflectionCacheSetClear()
    {
        // Uses a fresh ReflectionCacheSet instance to avoid mutating process-global shared state.
        var set = new ReflectionCacheSet();
        var cache = set.Internal.OpenGenericImplementationInfoByType;
        cache[typeof(Cached<>)] = OpenGenericImplementationInfo.For(typeof(Cached<>));

        set.Clear();

        Assert.Empty(cache);
    }

    [Fact]
    public void CacheEntriesAreRemovedByAMatchingReflectionCacheSetClear()
    {
        OpenGenericImplementationInfo.For(typeof(Cached<>));
        OpenGenericImplementationInfo.For(typeof(Kept<>));

        // The path taken when a collectible AssemblyLoadContext unloads. Only the entries the predicate
        // matches are removed, so this doesn't disturb other tests using the shared cache.
        ReflectionCacheSet.Shared.Clear((member, _) => member == typeof(Cached<>));

        var cache = ReflectionCacheSet.Shared.Internal.OpenGenericImplementationInfoByType;
        Assert.False(cache.ContainsKey(typeof(Cached<>)));
        Assert.True(cache.ContainsKey(typeof(Kept<>)));
    }

    private interface IService<T>
    {
    }

    private class Cached<T> : IService<T>
    {
    }

    private class Kept<T> : IService<T>
    {
    }
}
