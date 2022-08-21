// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Test.Util;

public class ReflectionCacheTests
{
    [Fact]
    public void ClearingReflectionCacheBetweenResolvesIsOk()
    {
        var builder = new ContainerBuilder();
        var customReflectionCache = new DefaultReflectionCache();
        builder.UseReflectionCache(customReflectionCache);

        builder.RegisterType<CDerivedSingle<int>>().As<ISingle<int>>();

        var container = builder.Build();

        container.Resolve<ISingle<int>>();

        customReflectionCache.Clear();

        container.Resolve<ISingle<int>>();
    }

    [Fact]
    public async Task ConcurrentCacheClearsAndResolvesIsOk()
    {
        var builder = new ContainerBuilder();
        var customReflectionCache = new DefaultReflectionCache();
        builder.UseReflectionCache(customReflectionCache);

        builder.RegisterType<CDerivedSingle<int>>().As<ISingle<int>>();

        var container = builder.Build();

        var resolveLoopTask = Task.Run(() =>
        {
            for (var index = 0; index < 10_000; index++)
            {
                container.Resolve<ISingle<int>>();
            }
        });

        var cacheClearTask = Task.Run(() =>
        {
            for (var index = 0; index < 1000; index++)
            {
                customReflectionCache.Clear((assembly, type) => type == typeof(CDerivedSingle<int>));
            }
        });

        await Task.WhenAll(resolveLoopTask, cacheClearTask);
    }

    [Fact]
    public void ClearingReflectionCacheBetweenOpenGenericResolvesIsOk()
    {
        var builder = new ContainerBuilder();
        var customReflectionCache = new DefaultReflectionCache();
        builder.UseReflectionCache(customReflectionCache);

        builder.RegisterGeneric(typeof(CDerivedSingle<>)).As(typeof(ISingle<>));

        var container = builder.Build();

        container.Resolve<ISingle<int>>();

        customReflectionCache.Clear();

        container.Resolve<ISingle<int>>();
    }

    private interface ISingle<T>
    {
    }

    private class CDerivedSingle<T> : ISingle<T>
    {
    }
}
