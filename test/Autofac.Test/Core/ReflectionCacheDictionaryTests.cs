// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Util.Cache;

namespace Autofac.Test.Core;

public class ReflectionCacheDictionaryTests
{
    [Fact]
    public void CanClearContents()
    {
        var cacheDict = new ReflectionCacheDictionary<Type, bool>();

        cacheDict[typeof(string)] = false;

        cacheDict.Clear();

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void CanConditionallyClearContents()
    {
        var cacheDict = new ReflectionCacheDictionary<Type, bool>();

        cacheDict[typeof(string)] = false;
        cacheDict[typeof(int)] = false;

        cacheDict.Clear((assembly, member) =>
        {
            Assert.Equal(typeof(string).Assembly, assembly);

            return member == typeof(string);
        });

        Assert.Collection(cacheDict, (kvp) => Assert.Equal(typeof(int), kvp.Key));
    }
}
