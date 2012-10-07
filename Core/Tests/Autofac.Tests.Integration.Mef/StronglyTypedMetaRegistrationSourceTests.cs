using System;
using System.ComponentModel.Composition;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class StronglyTypedMeta_WhenMetadataIsSupplied
    {
        const int SuppliedValue = 123;
        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            builder.RegisterType<object>().WithMetadata("TheInt", SuppliedValue);
            _container = builder.Build();
        }

        [Test]
        public void ValuesAreProvidedFromMetadata()
        {
            var meta = _container.Resolve<Meta<object, IMeta>>();
            Assert.AreEqual((int) SuppliedValue, (int) meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesProvidedFromMetadataOverrideDefaults()
        {
            var meta = _container.Resolve<Meta<object, IMetaWithDefault>>();
            Assert.AreEqual((int) SuppliedValue, (int) meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Meta<Func<object>, IMeta>>();
            Assert.AreEqual((int) SuppliedValue, (int) meta.Metadata.TheInt);
        }
    }

    [TestFixture]
    public class StronglyTypedMeta_WhenNoMatchingMetadataIsSupplied
    {
        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            var dx = Assert.Throws<DependencyResolutionException>(() => _container.Resolve<Meta<object, IMeta>>());
            Assert.IsInstanceOf<CompositionContractMismatchException>(dx.InnerException);
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Meta<object, IMetaWithDefault>>();
            Assert.AreEqual((int) 42, (int) m.Metadata.TheInt);
        }
    }
}