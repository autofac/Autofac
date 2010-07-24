using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class RegistrationBuilderTests
    {
        interface IProperties
        {
            int A { get; }
            string B { get; }
        }

        [Test]
        public void WhenPropetyFromStronglyTypedInterfaceConfigured_ReflectedInComponentRegistration()
        {
            var builder = RegistrationBuilder.ForType<object>();
            builder.WithMetadata<IProperties>(ep => ep
                .For(p => p.A, 42)
                .For(p => p.B, "hello"));
            
            var reg = builder.CreateRegistration();
            Assert.AreEqual(42, reg.Metadata["A"]);
            Assert.AreEqual("hello", reg.Metadata["B"]);
        }

        [Test]
        public void WhenAccessorNotPropertyAccessExpression_ArgumentExceptionThrown()
        {
            var builder = RegistrationBuilder.ForType<object>();
            Assert.Throws<ArgumentException>(() =>
                builder.WithMetadata<IProperties>(ep => ep.For(p => 42, 42)));
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
