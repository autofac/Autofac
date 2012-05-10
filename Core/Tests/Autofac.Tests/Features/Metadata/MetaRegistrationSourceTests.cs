using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Autofac.Tests.Features.Metadata
{
    [TestFixture]
    public class MetaRegistrationSourceTests
    {
        [Test]
        public void WhenGeneratingMetadata_ValuesProvidedFromMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 42);
            var container = builder.Build();
            var meta = container.Resolve<Meta<object>>();
            Assert.AreEqual(42, meta.Metadata["TheInt"]);
        }
    }
}
