using System;
using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Autofac.Tests.Features.Metadata
{
    class Metadata
    {
        public Metadata(int theInt)
        {
            TheInt = theInt;
        }

        public int TheInt { get; private set; }
    }

    [TestFixture]
    public class WhenMetadataIsSupplied
    {
        const int SuppliedValue = 123;
        IContainer _container;
        Metadata _metadata;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            _metadata = new Metadata(SuppliedValue);
            builder.RegisterType<object>().WithMetadata(_metadata);
            _container = builder.Build();
        }

        [Test]
        public void ValueIsProvidedFromMetadata()
        {
            var meta = _container.Resolve<Meta<object, Metadata>>();
            Assert.That(meta.Metadata, Is.EqualTo(_metadata));
        }

        [Test]
        public void ValueBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Meta<Func<object>, Metadata>>();
            Assert.That(meta.Metadata, Is.EqualTo(_metadata));
        }
    }
}
