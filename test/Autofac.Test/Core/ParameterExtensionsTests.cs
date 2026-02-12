// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Test.Core;

public class ParameterExtensionsTests
{
    [Fact]
    public void KeyedServiceKey_ReturnsValue()
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
}
