// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Test.Util.Cache;

public class ReflectionCacheAssemblyDictionaryTests
{
    [Fact]
    public void CanClearContents()
    {
        var cacheDict = new ReflectionCacheAssemblyDictionary<Assembly, bool>();

        cacheDict[typeof(string).Assembly] = false;

        cacheDict.Clear();

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void CanConditionallyClearContents()
    {
        var cacheDict = new ReflectionCacheAssemblyDictionary<Assembly, bool>();

        cacheDict[typeof(string).Assembly] = false;
        cacheDict[typeof(ContainerBuilder).Assembly] = false;

        cacheDict.Clear((assembly, member) =>
        {
            Assert.Null(member);

            return assembly == typeof(string).Assembly;
        });

        Assert.Collection(cacheDict, (kvp) => Assert.Equal(typeof(ContainerBuilder).Assembly, kvp.Key));
    }
}
