// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.Indexed;

namespace Autofac.Test.Features.Indexed;

public class KeyedServiceIndexTests
{
    [Fact]
    public void IndexerRetrievesComponentsFromContextByKey()
    {
        var key = 42;
        var cpt = "Hello";

        var idx = CreateTarget(cpt, key);

        Assert.Same(cpt, idx[key]);
    }

    [Fact]
    public void TryGetValueRetrievesComponentsFromContextByKey()
    {
        var key = 42;
        var cpt = "Hello";

        var idx = CreateTarget(cpt, key);

        Assert.True(idx.TryGetValue(key, out string val));
        Assert.Same(cpt, val);
    }

    private static KeyedServiceIndex<int, string> CreateTarget(string cpt, int key)
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(cpt).Keyed<string>(key);
        var container = builder.Build();

        return new KeyedServiceIndex<int, string>(container);
    }

    [Fact]
    public void WhenKeyNotFound_IndexerReturnsFalse()
    {
        var key = 42;
        var cpt = "Hello";

        var idx = CreateTarget(cpt, key);
        Assert.False(idx.TryGetValue(key + 1, out _));
    }
}
