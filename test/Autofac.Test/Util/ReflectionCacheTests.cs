using Autofac.Util;

namespace Autofac.Test.Util;

public class ReflectionCacheTests
{
    [Fact]
    public void ClearingReflectionCacheBetweenResolvesIsOk()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<CDerivedSingle<int>>().As<ISingle<int>>();

        var container = builder.Build();

        container.Resolve<ISingle<int>>();

        ReflectionCache.Clear();

        container.Resolve<ISingle<int>>();
    }

    [Fact]
    public async Task ConcurrentCacheClearsAndResolvesIsOk()
    {
        var builder = new ContainerBuilder();

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
                ReflectionCache.Clear((t) => t == typeof(CDerivedSingle<int>));
            }
        });

        await Task.WhenAll(resolveLoopTask, cacheClearTask);
    }

    [Fact]
    public void ClearingReflectionCacheBetweenOpenGenericResolvesIsOk()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneric(typeof(CDerivedSingle<>)).As(typeof(ISingle<>));

        var container = builder.Build();

        container.Resolve<ISingle<int>>();

        ReflectionCache.Clear();

        container.Resolve<ISingle<int>>();
    }

    [Fact]
    public void CanRegisterAndUnRegister()
    {
        int clearCount = 0;

        void ClearCallback(ReflectionCacheShouldClearPredicate predicate)
        {
            clearCount++;

            Assert.NotNull(predicate);
        }

        ReflectionCache.Register(ClearCallback);

        ReflectionCache.Clear(_ => true);

        Assert.Equal(1, clearCount);

        ReflectionCache.Unregister(ClearCallback);

        ReflectionCache.Clear(_ => true);

        Assert.Equal(1, clearCount);
    }

    private interface ISingle<T>
    {
    }

    private class CDerivedSingle<T> : ISingle<T>
    {
    }
}
