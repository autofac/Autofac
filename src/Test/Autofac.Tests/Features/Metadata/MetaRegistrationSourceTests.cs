using System;
using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Autofac.Tests.Features.Metadata
{
    public interface IMeta
    {
        int TheInt { get; }
    }

    [TestFixture]
    public class MetaRegistrationSourceTests
    {
        [Test]
        public void WhenGeneratingMetadata_ValuesProvidedFromMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 42);
            var container = builder.Build();
            var meta = container.Resolve<Meta<object, IMeta>>();
            Assert.AreEqual(42, meta.Metadata.TheInt);
        }

        [Test]
        public void WhenGeneratingMetadataOverContextAdapter_ValuesProvidedFromTargetMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 42);
            var container = builder.Build();
            var meta = container.Resolve<Meta<Func<object>, IMeta>>();
            Assert.AreEqual(42, meta.Metadata.TheInt);
        }
    }
}
