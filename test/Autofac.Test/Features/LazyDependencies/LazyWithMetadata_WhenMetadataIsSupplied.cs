// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.Features.Metadata.TestTypes;

namespace Autofac.Test.Features.LazyDependencies;

public class LazyWithMetadata_WhenMetadataIsSupplied
{
    private const int SuppliedValue = 123;
    private IContainer _container;

    public LazyWithMetadata_WhenMetadataIsSupplied()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().WithMetadata("TheInt", SuppliedValue);
        _container = builder.Build();
    }

    [Fact]
    public void ValuesAreProvidedFromMetadata()
    {
        var meta = _container.Resolve<Lazy<object, MyMeta>>();
        Assert.Equal(SuppliedValue, meta.Metadata.TheInt);
    }

    [Fact]
    public void ValuesProvidedAreUniqueToEachRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().WithMetadata("TheInt", 123);
        builder.RegisterType<string>().WithMetadata("TheInt", 321);
        _container = builder.Build();

        var meta1 = _container.Resolve<Lazy<object, MyMeta>>();
        Assert.Equal(123, meta1.Metadata.TheInt);

        var meta2 = _container.Resolve<Lazy<string, MyMeta>>();
        Assert.Equal(321, meta2.Metadata.TheInt);
    }

    [Fact]
    public void ValuesProvidedFromMetadataOverrideDefaults()
    {
        var meta = _container.Resolve<Lazy<object, MyMetaWithDefault>>();
        Assert.Equal(SuppliedValue, meta.Metadata.TheInt);
    }

    [Fact]
    public void MutableDefaultValuesAreNotSharedBetweenRegistrations()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>();
        builder.RegisterType<object>();
        var container = builder.Build();

        var lazies = container.Resolve<IEnumerable<Lazy<object, MyMetaWithMutableDefault>>>().ToArray();
        lazies[0].Metadata.TheValues![0] = 99;

        Assert.Equal(new[] { 1, 2 }, lazies[1].Metadata.TheValues);
    }

    [Fact]
    public void ValueIsResolvedOnlyWhenRequested()
    {
        var builder = new ContainerBuilder();
        var activations = 0;
        builder.Register(c =>
        {
            activations++;
            return new object();
        }).WithMetadata("TheInt", SuppliedValue);
        _container = builder.Build();

        var lazy = _container.Resolve<Lazy<object, MyMeta>>();

        Assert.IsType<Lazy<object, MyMeta>>(lazy);
        Assert.Equal(0, activations);
        Assert.Same(lazy.Value, lazy.Value);
        Assert.Equal(1, activations);
    }

    [Fact]
    public void ValuesBubbleUpThroughAdapters()
    {
        var meta = _container.Resolve<Lazy<Func<object>, MyMeta>>();
        Assert.Equal(SuppliedValue, meta.Metadata.TheInt);
    }
}
