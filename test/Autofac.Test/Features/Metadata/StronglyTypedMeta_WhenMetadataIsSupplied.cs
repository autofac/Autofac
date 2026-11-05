// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;
using Autofac.Test.Features.Metadata.TestTypes;

namespace Autofac.Test.Features.Metadata;

public class StronglyTypedMeta_WhenMetadataIsSupplied
{
    private const int SuppliedIntValue = 123;
    private const string SuppliedNameValue = "Homer";

    private IContainer _container;

    public StronglyTypedMeta_WhenMetadataIsSupplied()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>()
            .WithMetadata("TheInt", SuppliedIntValue)
            .WithMetadata("Name", SuppliedNameValue);
        _container = builder.Build();
    }

    [Fact]
    public void ValuesAreProvidedFromMetadata()
    {
        var meta = _container.Resolve<Meta<object, MyMeta>>();
        Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
    }

    [Fact]
    public void ValuesProvidedAreUniqueToEachRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>().WithMetadata("TheInt", 123);
        builder.RegisterType<string>().WithMetadata("TheInt", 321);
        _container = builder.Build();

        var meta1 = _container.Resolve<Meta<object, MyMeta>>();
        Assert.Equal(123, meta1.Metadata.TheInt);

        var meta2 = _container.Resolve<Meta<string, MyMeta>>();
        Assert.Equal(321, meta2.Metadata.TheInt);
    }

    [Fact]
    public void ValuesProvidedAreUniqueToEachRegistrationInSiblingScopes()
    {
        using var scope1 = _container.BeginLifetimeScope(b => b.RegisterType<string>().WithMetadata("TheInt", 1));
        using var scope2 = _container.BeginLifetimeScope(b => b.RegisterType<string>().WithMetadata("TheInt", 2));

        Assert.Equal(1, scope1.Resolve<Meta<string, MyMeta>>().Metadata.TheInt);
        Assert.Equal(2, scope2.Resolve<Meta<string, MyMeta>>().Metadata.TheInt);
        Assert.Equal(1, scope1.Resolve<Lazy<string, MyMeta>>().Metadata.TheInt);
        Assert.Equal(2, scope2.Resolve<Lazy<string, MyMeta>>().Metadata.TheInt);
    }

    [Fact]
    public void EachResolveCreatesANewMetadataView()
    {
        var meta1 = _container.Resolve<Meta<object, MyMeta>>();
        var meta2 = _container.Resolve<Meta<object, MyMeta>>();

        Assert.NotSame(meta1.Metadata, meta2.Metadata);
    }

    [Fact]
    public void MetadataViewFactoryIsCompiledOncePerMetadataType()
    {
        var factories = ReflectionCacheSet.Shared.Internal.MetadataViewFactories;

        MetadataViewProvider.GetMetadataViewProvider<MyMeta>();
        var factory = factories[typeof(MyMeta)].Factory;
        MetadataViewProvider.GetMetadataViewProvider<MyMeta>();
        MetadataViewProvider.GetMetadataViewProvider<MyMetaWithDefault>();

        Assert.Same(factory, factories[typeof(MyMeta)].Factory);
        Assert.NotSame(factory, factories[typeof(MyMetaWithDefault)].Factory);
    }

    [Fact]
    public void MetadataViewFactoryCacheIsClearedByReflectionCacheSetClear()
    {
        // Uses a fresh ReflectionCacheSet instance to avoid mutating process-global shared state.
        var set = new ReflectionCacheSet();
        var cache = set.Internal.MetadataViewFactories;
        cache[typeof(MyMeta)] = default;

        set.Clear();

        Assert.Empty(cache);
    }

    [Fact]
    public void MetadataViewFactoryCacheIsClearedByReflectionCacheSetPredicateClear()
    {
        // Uses a fresh ReflectionCacheSet instance to avoid mutating process-global shared state.
        var set = new ReflectionCacheSet();
        var cache = set.Internal.MetadataViewFactories;
        cache[typeof(MyMeta)] = default;
        cache[typeof(MyMetaWithDefault)] = default;

        set.Clear((member, _) => member == typeof(MyMeta));

        Assert.False(cache.ContainsKey(typeof(MyMeta)));
        Assert.True(cache.ContainsKey(typeof(MyMetaWithDefault)));
    }

    [Fact]
    public void MutableDefaultValuesAreNotSharedBetweenRegistrations()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<object>();
        builder.RegisterType<object>();
        var container = builder.Build();

        var metas = container.Resolve<IEnumerable<Meta<object, MyMetaWithMutableDefault>>>().ToArray();
        metas[0].Metadata.TheValues![0] = 99;

        Assert.Equal(new[] { 1, 2 }, metas[1].Metadata.TheValues);
    }

    [Fact]
    public void ValuesProvidedFromMetadataOverrideDefaults()
    {
        var meta = _container.Resolve<Meta<object, MyMetaWithDefault>>();
        Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
    }

    [Fact]
    public void ValuesProvidedToTypesWithDictionaryConstructor()
    {
        var meta = _container.Resolve<Meta<object, MyMetaWithDictionary>>();
        Assert.Equal(SuppliedNameValue, meta.Metadata.TheName);
    }

    [Fact]
    public void ReadOnlyPropertiesOnMetadataViewAreIgnored()
    {
        var meta = _container.Resolve<Meta<object, MyMetaWithReadOnlyProperty>>();
        Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
    }

    [Fact]
    public void ResolvingStronglyTypedMetadataWithInvalidConstructorThrowsException()
    {
        var exception = Assert.Throws<DependencyResolutionException>(
            () => _container.Resolve<Meta<object, MyMetaWithInvalidConstructor>>());

        var typeName = typeof(MyMetaWithInvalidConstructor).Name;
        var message = string.Format(CultureInfo.InvariantCulture, MetadataViewProviderResources.InvalidViewImplementation, typeName);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ResolvingStronglyTypedMetadataWithInterfaceThrowsException()
    {
        Assert.Throws<ComponentNotRegisteredException>(
            () => _container.Resolve<Meta<object, IMyMetaInterface>>());
    }

    [Fact]
    public void ValuesBubbleUpThroughAdapters()
    {
        var meta = _container.Resolve<Meta<Func<object>, MyMeta>>();
        Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
    }
}
