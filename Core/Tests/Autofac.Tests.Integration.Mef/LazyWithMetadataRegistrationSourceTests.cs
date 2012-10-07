﻿using System;
﻿using System.ComponentModel.Composition;
using Autofac.Core;
using NUnit.Framework;
﻿using Autofac.Integration.Mef;

namespace Autofac.Tests.Integration.Mef
{
    [TestFixture]
    public class LazyWithMetadata_WhenMetadataIsSupplied
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
    public class LazyWithMetadata_WhenNoMatchingMetadataIsSupplied
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
            var dx = Assert.Throws<DependencyResolutionException>(() => _container.Resolve<Lazy<object, IMeta>>());

            Assert.IsInstanceOf<CompositionContractMismatchException>(dx.InnerException);
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Lazy<object, IMetaWithDefault>>();
            Assert.AreEqual(42, m.Metadata.TheInt);
        }
    }
}