// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit;
#nullable enable

namespace Autofac.Test.Compilation
{
    public class NullableReferenceTests
    {
        [Fact]
        public void NullableTypeNotAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register<SimpleReferenceType?>(c => new SimpleReferenceType());
                ")
                .AssertWarningContainsKeywords("Autofac.Test.Compilation.SimpleReferenceType?", "notnull");
        }

        [Fact]
        public void NullableTypeNotAllowedForAsMethod()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).As<BaseClass?>();
                ")
                .AssertWarningContainsKeywords("Autofac.Test.Compilation.BaseClass?", "notnull");
        }

        [Fact]
        public void NullableTypeNotAllowedForNamedService()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).Named<BaseClass?>(""name"");
                ")
                .AssertWarningContainsKeywords("Autofac.Test.Compilation.BaseClass?", "notnull");
        }

        [Fact]
        public void NullableTypeNotAllowedForKeyedService()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).Keyed<BaseClass?>(""name"");
                ")
                .AssertWarningContainsKeywords("Autofac.Test.Compilation.BaseClass?", "notnull");
        }

        [Fact]
        public void NullableSystemTypeNotAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register<int?>(c => 1);
                ")
                .AssertWarningContainsKeywords("int?", "notnull");
        }

        [Fact]
        public void NonNullableTypeAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new SimpleReferenceType());
                ")
                .AssertNoWarnings();
        }

        [Fact]
        public void TryResolveGeneratesNullWarning()
        {
            new AutofacCompile()
                .Body(
                @"
                    var containerBuilder = new ContainerBuilder();

                    var container = containerBuilder.Build();

                    container.TryResolve<SimpleReferenceType>(out var simpleType);

                    simpleType.ToString();
                ")
                .AssertWarningContainsKeywords("null");
        }
    }
}
