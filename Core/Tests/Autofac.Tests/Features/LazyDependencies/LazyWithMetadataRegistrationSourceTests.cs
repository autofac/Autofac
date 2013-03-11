﻿using System;
﻿using Autofac.Core;
﻿using Autofac.Tests.Builder;
﻿using Autofac.Tests.Features.Metadata;
﻿using NUnit.Framework;

namespace Autofac.Tests.Features.LazyDependencies
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
            builder.RegisterType<object>().WithMetadata("TheInt", SuppliedValue);
            _container = builder.Build();
        }

        [Test]
        public void ValuesAreProvidedFromMetadata()
        {
            var meta = _container.Resolve<Lazy<object, MyMeta>>();
            Assert.AreEqual(SuppliedValue, meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesProvidedAreUniqueToEachRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 123);
            builder.RegisterType<string>().WithMetadata("TheInt", 321);
            _container = builder.Build();

            var meta1 = _container.Resolve<Lazy<object, MyMeta>>();
            Assert.That(meta1.Metadata.TheInt, Is.EqualTo(123));

            var meta2 = _container.Resolve<Lazy<string, MyMeta>>();
            Assert.That(meta2.Metadata.TheInt, Is.EqualTo(321));
        }

        [Test]
        public void ValuesProvidedFromMetadataOverrideDefaults()
        {
            var meta = _container.Resolve<Lazy<object, MyMetaWithDefault>>();
            Assert.AreEqual(SuppliedValue, meta.Metadata.TheInt);
        }

        [Test]
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Lazy<Func<object>, MyMeta>>();
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
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            Assert.Throws<DependencyResolutionException>(() => _container.Resolve<Lazy<object, MyMeta>>());
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Lazy<object, MyMetaWithDefault>>();
            Assert.AreEqual(42, m.Metadata.TheInt);
        }
    }
}