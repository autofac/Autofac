#if !NET451
using System;
using Autofac.Core;
using Autofac.Test.Features.Metadata;
using Autofac.Test.Features.Metadata.TestTypes;
using Xunit;

namespace Autofac.Test.Features.LazyDependencies
{
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
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Lazy<Func<object>, MyMeta>>();
            Assert.Equal(SuppliedValue, meta.Metadata.TheInt);
        }
    }
}
#endif