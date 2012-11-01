using System;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;
using Autofac.Util;
using NUnit.Framework;

namespace Autofac.Tests.Features.Metadata
{
    [TestFixture]
    public class StronglyTypedMeta_WhenMetadataIsSupplied
    {
        const int SuppliedIntValue = 123;
        const string SuppliedNameValue = "Homer";

        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .WithMetadata("TheInt", SuppliedIntValue)
                .WithMetadata("Name", SuppliedNameValue);
            _container = builder.Build();
        }

        [Test]
        public void ValuesAreProvidedFromMetadata()
        {
            var meta = _container.Resolve<Meta<object, MyMeta>>();
            Assert.That(meta.Metadata.TheInt, Is.EqualTo(SuppliedIntValue));
        }

        [Test]
        public void ValuesProvidedFromMetadataOverrideDefaults()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithDefault>>();
            Assert.That(meta.Metadata.TheInt, Is.EqualTo(SuppliedIntValue));
        }

        [Test]
        public void ValuesProvidedToTypesWithDictionaryConstructor()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithDictionary>>();
            Assert.That(meta.Metadata.TheName, Is.EqualTo(SuppliedNameValue));
        }

        [Test]
        public void ReadOnlyPropertiesOnMetadataViewAreIgnored()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithReadOnlyProperty>>();
            Assert.That(meta.Metadata.TheInt, Is.EqualTo(SuppliedIntValue));
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithInvalidConstructorThrowsException()
        {
            var exception = Assert.Throws<DependencyResolutionException>(
                () => _container.Resolve<Meta<object, MyMetaWithInvalidConstructor>>());

            var typeName = typeof(MyMetaWithInvalidConstructor).Name;
            var message = string.Format(MetadataViewProviderResources.InvalidViewImplementation, typeName);

            Assert.That(exception.Message, Is.EqualTo(message));
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithInterfaceThrowsException()
        {
            Assert.Throws<ComponentNotRegisteredException>(
                () => _container.Resolve<Meta<object, IMyMetaInterface>>());
        }

        [Test]
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Meta<Func<object>, MyMeta>>();
            Assert.That(meta.Metadata.TheInt, Is.EqualTo(SuppliedIntValue));
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
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            var exception = Assert.Throws<DependencyResolutionException>(
                () => _container.Resolve<Meta<object, MyMeta>>());

            var propertyName = ReflectionExtensions.GetProperty<MyMeta, int>(x => x.TheInt).Name;
            var message = string.Format(MetadataViewProviderResources.MissingMetadata, propertyName);

            Assert.That(exception.Message, Is.EqualTo(message));
        }

        [Test]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Meta<object, MyMetaWithDefault>>();
            Assert.That(m.Metadata.TheInt, Is.EqualTo(42));
        }
    }
}