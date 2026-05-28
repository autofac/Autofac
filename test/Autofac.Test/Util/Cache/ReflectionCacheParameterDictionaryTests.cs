// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Util.Cache;

namespace Autofac.Test.Util.Cache;

public class ReflectionCacheParameterDictionaryTests
{
    private static readonly ParameterInfo _sampleParameter = typeof(ParameterOwner)
        .GetMethod(nameof(ParameterOwner.Method), BindingFlags.Public | BindingFlags.Static)!
        .GetParameters()
        .Single();

    [Fact]
    public void Usage_DefaultsToAll()
    {
        var cache = new ReflectionCacheParameterDictionary<bool>();

        Assert.Equal(ReflectionCacheUsage.All, cache.Usage);
    }

    [Fact]
    public void Usage_CanBeUpdated()
    {
        var cache = new ReflectionCacheParameterDictionary<bool>
        {
            Usage = ReflectionCacheUsage.None,
        };

        Assert.Equal(ReflectionCacheUsage.None, cache.Usage);
    }

    [Fact]
    public void Clear_NullPredicate()
    {
        var cache = new ReflectionCacheParameterDictionary<bool>();

        Assert.Throws<ArgumentNullException>(() => cache.Clear(null!));
    }

    [Fact]
    public void Clear_PredicateDoesNotMatch()
    {
        var cache = new ReflectionCacheParameterDictionary<bool>
        {
            [_sampleParameter] = true,
        };

        cache.Clear((_, _) => false);

        Assert.True(cache.ContainsKey(_sampleParameter));
    }

    [Fact]
    public void Clear_PredicateMatchesMember()
    {
        var cache = new ReflectionCacheParameterDictionary<bool>
        {
            [_sampleParameter] = true,
        };

        cache.Clear((member, _) => member == _sampleParameter.Member);

        Assert.False(cache.ContainsKey(_sampleParameter));
    }

    private static class ParameterOwner
    {
        public static void Method(object value)
        {
        }
    }
}
