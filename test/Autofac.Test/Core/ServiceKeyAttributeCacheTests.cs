// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Test.Core;

public class ServiceKeyAttributeCacheTests
{
    [Fact]
    public void ParameterHasServiceKey_FindsConstructorParameter()
    {
        var constructor = typeof(NeedsConstructorKey).GetConstructors().Single();
        var parameter = constructor.GetParameters().Single();

        Assert.True(ServiceKeyAttributeCache.ParameterHasServiceKey(parameter));
    }

    [Fact]
    public void ParameterHasServiceKey_NullParameter()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceKeyAttributeCache.ParameterHasServiceKey(null!));
    }

    [Fact]
    public void PropertyHasServiceKey_FindsProperty()
    {
        var property = typeof(NeedsPropertyKey).GetProperty(nameof(NeedsPropertyKey.Dependency))!;
        var setterParameter = property.SetMethod!.GetParameters().Single();

        Assert.True(ServiceKeyAttributeCache.PropertyHasServiceKey(property));
        Assert.True(ServiceKeyAttributeCache.ParameterHasServiceKey(setterParameter));
    }

    [Fact]
    public void PropertyHasServiceKey_NullProperty()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceKeyAttributeCache.PropertyHasServiceKey(null!));
    }

    private sealed class NeedsConstructorKey
    {
        public NeedsConstructorKey([ServiceKey] object key)
        {
            Key = key;
        }

        public object Key
        {
            get;
        }
    }

    private sealed class NeedsPropertyKey
    {
        [ServiceKey]
        public object Dependency { get; set; } = default!;
    }
}
