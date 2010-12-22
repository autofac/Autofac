using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using NUnit.Framework;

namespace Autofac.Tests.Features.LazyDependencies
{
    public interface IMeta
    {
        int TheInt { get; }
    }

    public interface IMetaWithDefault
    {
        [DefaultValue(42)]
        int TheInt { get; }
    }

    [TestFixture]
    public class WhenMetadataIsSupplied
    {
        const int SuppliedValue = 123;
        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", SuppliedValue);
            _container = builder.Build();
        }

        [Test]
        public void ValuesAreProvidedFromMetadata()
        {
            var meta = _container.Resolve<Lazy<object, IMeta>>();
            Assert.AreEqual(SuppliedValue, meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesProvidedFromMetadataOverrideDefaults()
        {
            var meta = _container.Resolve<Lazy<object, IMetaWithDefault>>();
            Assert.AreEqual(SuppliedValue, meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Lazy<Func<object>, IMeta>>();
            Assert.AreEqual(SuppliedValue, meta.Metadata.TheInt);
        }
    }

    [TestFixture]
    public class WhenNoMatchingMetadataIsSupplied
    {
        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            Assert.Throws<CompositionContractMismatchException>(() =>
                _container.Resolve<Lazy<object, IMeta>>());
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Lazy<object, IMetaWithDefault>>();
            Assert.AreEqual(42, m.Metadata.TheInt);
        }
    }
}
