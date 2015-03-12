using Autofac.Features.Metadata;
using Xunit;

namespace Autofac.Test.Features.Metadata
{
    public class MetaRegistrationSourceTests
    {
        [Fact]
        public void WhenGeneratingMetadata_ValuesProvidedFromMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 42);
            var container = builder.Build();
            var meta = container.Resolve<Meta<object>>();
            Assert.Equal(42, meta.Metadata["TheInt"]);
        }
    }
}
