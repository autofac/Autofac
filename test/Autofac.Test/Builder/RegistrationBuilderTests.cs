﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Builder;

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

        using var reg = builder.CreateRegistration();

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
        using var registration = RegistrationBuilder.ForType<object>()
            .As(Array.Empty<Service>())
            .CreateRegistration();

        Assert.Empty(registration.Services);
    }
}
