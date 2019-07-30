using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Test.Features.Metadata.TestTypes;
using Autofac.Util;
using Xunit;

namespace Autofac.Test.Features.Metadata
{
    public class StronglyTypedMeta_WhenNoMatchingMetadataIsSupplied
    {
        private readonly IContainer _container;

        public StronglyTypedMeta_WhenNoMatchingMetadataIsSupplied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            var exception = Assert.Throws<DependencyResolutionException>(
                () => _container.Resolve<Meta<object, MyMeta>>());

            var propertyName = ReflectionExtensions.GetProperty<MyMeta, int>(x => x.TheInt).Name;
            var message = string.Format(MetadataViewProviderResources.MissingMetadata, propertyName);

            Assert.Equal(message, exception.InnerException.Message);
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Meta<object, MyMetaWithDefault>>();
            Assert.Equal(42, m.Metadata.TheInt);
        }
    }
}