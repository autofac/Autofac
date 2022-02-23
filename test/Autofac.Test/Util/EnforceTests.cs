// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Util;

namespace Autofac.Test.Util;

public class EnforceTests
{
    [Fact]
    public void FindsEmptyElementInList()
    {
        Assert.Throws<ArgumentException>(() =>
            Enforce.ArgumentElementNotNull(new object[] { null }, "arg"));
    }
}
