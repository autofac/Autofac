#if !NET451
﻿using System;
﻿using Autofac.Core;
﻿using Autofac.Test.Features.Metadata;
﻿using Xunit;

namespace Autofac.Test.Features.LazyDependencies
{
    public class LazyWithMetadata_WhenMetadataIsSupplied
    {
        const int SuppliedValue = 123;
        IContainer _container;

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
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Lazy<Func<object>, MyMeta>>();
            Assert.Equal(SuppliedValue, meta.Metadata.TheInt);
        }
    }

    public class LazyWithMetadata_WhenNoMatchingMetadataIsSupplied
    {
        readonly IContainer _container;

        public LazyWithMetadata_WhenNoMatchingMetadataIsSupplied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            Assert.Throws<DependencyResolutionException>(() => _container.Resolve<Lazy<object, MyMeta>>());
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Lazy<object, MyMetaWithDefault>>();
            Assert.Equal(42, m.Metadata.TheInt);
        }
    }
}
#endif