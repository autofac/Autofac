// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Test.Core;

public class ParameterExtensionsTests
{
    [Fact]
    public void KeyedServiceKey_Found()
    {
        var result = new Parameter[] { new KeyedServiceKeyParameter("expected") }
            .KeyedServiceKey<string>();

        Assert.Equal("expected", result);
    }

    [Fact]
    public void KeyedServiceKey_NotFound()
    {
        Assert.Throws<InvalidOperationException>(
            () => Array.Empty<Parameter>().KeyedServiceKey<string>());
    }

    [Fact]
    public void TryGetKeyedServiceKey_Found()
    {
        var parameters = new Parameter[] { new KeyedServiceKeyParameter("expected") };

        var result = parameters.TryGetKeyedServiceKey(out string value);

        Assert.True(result);
        Assert.Equal("expected", value);
    }

    [Fact]
    public void TryGetKeyedServiceKey_NotFound()
    {
        var parameters = Array.Empty<Parameter>();

        var result = parameters.TryGetKeyedServiceKey(out string value);

        Assert.False(result);
        Assert.Null(value);
    }
}
