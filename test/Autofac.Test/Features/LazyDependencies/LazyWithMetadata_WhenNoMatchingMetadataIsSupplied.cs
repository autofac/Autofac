#if !NET451
using System;
using Autofac.Core;
using Autofac.Test.Features.Metadata;
using Autofac.Test.Features.Metadata.TestTypes;
using Xunit;

namespace Autofac.Test.Features.LazyDependencies
{
    public class LazyWithMetadata_WhenNoMatchingMetadataIsSupplied
    {
        private readonly IContainer _container;

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