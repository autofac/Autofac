using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class RegistrationBuilderTests
    {
        class TestMetadata
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        [Test]
        public void StronglyTypedInstanceConfigured_ReflectedInComponentRegistration()
        {
            var builder = RegistrationBuilder.ForType<object>();
            builder.WithMetadata(new TestMetadata {A = 42, B = "hello"});
            
            var reg = builder.CreateRegistration();
            var metadata = (TestMetadata)reg.Metadata[StronglyTypedMetaRegistrationSource.DictionaryKey];

            Assert.That(metadata.A, Is.EqualTo(42));
            Assert.That(metadata.B, Is.EqualTo("hello"));
        }

        [Test]
        public void WhenInstanceIsNull_ArgumentExceptionThrown()
        {
            var builder = RegistrationBuilder.ForType<object>();
            Assert.Throws<ArgumentNullException>(() => builder.WithMetadata(null));
        }

        [Test]
        public void AsEmptyList_CreatesRegistrationWithNoServices()
        {
            var registration = RegistrationBuilder.ForType<object>()
                .As(new Service[0])
                .CreateRegistration();

            Assert.AreEqual(0, registration.Services.Count());
        }
    }
}
