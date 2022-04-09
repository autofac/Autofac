using Autofac.Util;

namespace Autofac.Test.Util;

public class ReflectionCacheDictionaryTests
{
    [Fact]
    public void CanClearContents()
    {
        var cacheDict = new ReflectionCacheDictionary<Type, bool>(doNotAutoRegister: true);

        cacheDict[typeof(string)] = false;

        cacheDict.CacheClear(predicate: null);

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void CanConditionallyClearContents()
    {
        var cacheDict = new ReflectionCacheDictionary<Type, bool>(doNotAutoRegister: true);

        cacheDict[typeof(string)] = false;
        cacheDict[typeof(int)] = false;

        cacheDict.CacheClear(member => member == typeof(string));

        Assert.Collection(cacheDict, (kvp) => Assert.Equal(typeof(int), kvp.Key));
    }
}
