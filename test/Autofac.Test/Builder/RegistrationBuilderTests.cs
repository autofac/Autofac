// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using Xunit;

namespace Autofac.Test.Builder
{
    public class RegistrationBuilderTests
    {
        internal class TestMetadata
        {
            public int A { get; set; }

            public string B { get; set; }
        }

        [Fact]
        public void WhenPropertyFromStronglyTypedClassConfigured_ReflectedInComponentRegistration()
        {
            var builder = RegistrationBuilder.ForType<object>();
            builder.WithMetadata<TestMetadata>(ep => ep
                .For(p => p.A, 42)
                .For(p => p.B, "hello"));

            var reg = builder.CreateRegistration();

            Assert.Equal(42, reg.Metadata["A"]);
            Assert.Equal("hello", reg.Metadata["B"]);
        }

        [Fact]
        public void WhenAccessorNotPropertyAccessExpression_ArgumentExceptionThrown()
        {
            var builder = RegistrationBuilder.ForType<object>();
            Assert.Throws<ArgumentException>(() =>
                builder.WithMetadata<TestMetadata>(ep => ep.For(p => 42, 42)));
        }

        [Fact]
        public void AsEmptyList_CreatesRegistrationWithNoServices()
        {
            var registration = RegistrationBuilder.ForType<object>()
                .As(new Service[0])
                .CreateRegistration();

            Assert.Empty(registration.Services);
        }
    }
}
