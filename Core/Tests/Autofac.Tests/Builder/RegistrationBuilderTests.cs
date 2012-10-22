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
        public void WhenPropetyFromStronglyTypedClassConfigured_ReflectedInComponentRegistration()
        {
            var builder = RegistrationBuilder.ForType<object>();
            builder.WithMetadata<TestMetadata>(ep => ep
                .For(p => p.A, 42)
                .For(p => p.B, "hello"));

            var reg = builder.CreateRegistration();

            Assert.That(reg.Metadata["A"], Is.EqualTo(42));
            Assert.That(reg.Metadata["B"], Is.EqualTo("hello"));
        }

        [Test]
        public void WhenAccessorNotPropertyAccessExpression_ArgumentExceptionThrown()
        {
            var builder = RegistrationBuilder.ForType<object>();
            Assert.Throws<ArgumentException>(() =>
                builder.WithMetadata<TestMetadata>(ep => ep.For(p => 42, 42)));
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
