// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Test.Util.Cache;

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

    [Fact]
    public void MethodInfoAssemblyIsCorrect()
    {
        var cacheDict = new ReflectionCacheDictionary<MethodInfo, bool>();

        cacheDict[typeof(string).GetMethod("IsNullOrEmpty")] = false;

        cacheDict.Clear((assembly, member) =>
        {
            Assert.Equal(typeof(string).Assembly, assembly);

            return member == typeof(string);
        });

        Assert.Empty(cacheDict);
    }
}
