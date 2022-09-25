// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Util.Cache;

namespace Autofac.Test.Util.Cache;

public class ReflectionCacheTupleDictionaryTests
{
    [Fact]
    public void CanClearContents()
    {
        var cacheDict = new ReflectionCacheTupleDictionary<Type, bool>();

        cacheDict[(typeof(string), typeof(ContainerBuilder))] = false;

        cacheDict.Clear();

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void ClearsContentsWhenOneOfTheTupleMatches()
    {
        var cacheDict = new ReflectionCacheTupleDictionary<Type, bool>();

        cacheDict[(typeof(string), typeof(ContainerBuilder))] = false;

        cacheDict.Clear((assembly, member) =>
        {
            return member == typeof(string);
        });

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void ClearsContentsWhenBothOfTheTupleMatches()
    {
        var cacheDict = new ReflectionCacheTupleDictionary<Type, bool>();

        cacheDict[(typeof(string), typeof(int))] = false;

        cacheDict.Clear((assembly, member) =>
        {
            return member == typeof(int) || member == typeof(string);
        });

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void MethodInfoUsesContainingTypeAssembly()
    {
        var cacheDict = new ReflectionCacheTupleDictionary<MethodInfo, bool>();

        cacheDict[(typeof(string).GetMethod("IsNullOrEmpty"), typeof(int).GetMethod("GetHashCode"))] = false;

        cacheDict.Clear((assembly, member) =>
        {
            Assert.Equal(typeof(string).Assembly, assembly);

            return true;
        });

        Assert.Empty(cacheDict);
    }

    [Fact]
    public void TypeUsesTypeAssembly()
    {
        var cacheDict = new ReflectionCacheTupleDictionary<Type, bool>();

        cacheDict[(typeof(string), typeof(int))] = false;

        cacheDict.Clear((assembly, member) =>
        {
            Assert.Equal(typeof(string).Assembly, assembly);

            return true;
        });

        Assert.Empty(cacheDict);
    }
}
