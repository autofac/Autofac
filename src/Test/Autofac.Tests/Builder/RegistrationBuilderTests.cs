using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
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
            builder.WithMetadata<IProperties>(ep =>
            {
                ep.For(p => p.A, 42);
                ep.For(p => p.B, "hello");
            });
            
            var reg = RegistrationBuilder.CreateRegistration(builder);
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
    }
}
