// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.Scenarios.ScannedAssembly;

namespace Autofac.Test;

public class TypeExtensionsTests
{
    [Fact]
    public void IsClosedTypeOfNonGenericTypeProvidedThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            typeof(object).IsClosedTypeOf(typeof(SaveCommandData)));
    }

    [Fact]
    public void IsClosedTypeOfClosedGenericTypeProvidedThrowsException()
    {
        var cb = new ContainerBuilder();
        Assert.Throws<ArgumentException>(() =>
            typeof(object).IsClosedTypeOf(typeof(ICommand<SaveCommandData>)));
    }

    [Fact]
    public void IsClosedTypeOfReturnsTrueForOpenGenericInterfaces()
    {
        Assert.True(typeof(ICommand<SaveCommandData>).IsClosedTypeOf(typeof(ICommand<>)));
    }

    [Fact]
    public void IsClosedTypeOfReturnsTrueForClosedClasses()
    {
        Assert.True(typeof(SaveCommand).IsClosedTypeOf(typeof(ICommand<>)));
    }

    [Fact]
    public void AnOpenGenericTypeIsNotAClosedTypeOfAnything()
    {
        Assert.False(typeof(CommandBase<>).IsClosedTypeOf(typeof(CommandBase<>)));
    }

    [Theory]
    [InlineData(typeof(TypeExtensionsTests), "Autofac", true)]
    [InlineData(typeof(TypeExtensionsTests), "Autofac.Test", true)]
    [InlineData(typeof(TypeExtensionsTests), "Auto", false)]
    [InlineData(typeof(TypeExtensionsTests), "", false)]
    public void IsInNamespace_DetectsNamespace(Type t, string ns, bool expected)
    {
        Assert.Equal(expected, t.IsInNamespace(ns));
    }

    [Fact]
    public void IsInNamespaceOf_SameNamespace()
    {
        Assert.True(typeof(TypeExtensionsTests).IsInNamespaceOf<TypedParameterTests>());
    }

    [Fact]
    public void IsInNamespaceOf_DifferentNamespace()
    {
        Assert.False(typeof(ContainerBuilder).IsInNamespaceOf<TypeExtensionsTests>());
    }

    [Theory]
    [InlineData("PublicInstanceMethod")]
    [InlineData("PublicStaticMethod")]
    [InlineData("ProtectedInstanceMethod")]
    [InlineData("ProtectedStaticMethod")]
    [InlineData("PrivateInstanceMethod")]
    [InlineData("PrivateStaticMethod")]
    public void GetDeclaredMethod_FindsMethods(string method)
    {
        Assert.NotNull(typeof(DeclaredMethodType).GetDeclaredMethod(method));
    }

    [Fact]
    public void GetDeclaredMethod_MissingMethod()
    {
        Assert.Throws<ArgumentException>(() => typeof(DeclaredMethodType).GetDeclaredMethod("NoSuchMethod"));
    }

    [Theory]
    [InlineData("PublicInstanceProperty")]
    [InlineData("PublicStaticProperty")]
    [InlineData("ProtectedInstanceProperty")]
    [InlineData("ProtectedStaticProperty")]
    [InlineData("PrivateInstanceProperty")]
    [InlineData("PrivateStaticProperty")]
    public void GetDeclaredProperty_FindsProperties(string property)
    {
        Assert.NotNull(typeof(DeclaredPropertyType).GetDeclaredProperty(property));
    }

    [Fact]
    public void GetDeclaredProperty_MissingProperty()
    {
        Assert.Throws<ArgumentException>(() => typeof(DeclaredPropertyType).GetDeclaredProperty("NoSuchProperty"));
    }

    [Fact]
    public void GetDeclaredConstructors_OnlyFindsInstanceConstructors()
    {
        // Issue #1238 - Don't consider static constructors during DI.
        Assert.Equal(4, typeof(DeclaredConstructorType).GetDeclaredConstructors().Length);
    }

    [Fact]
    public void GetDeclaredConstructors_FindsDefaultInstanceConstructors()
    {
        Assert.Single(typeof(DefaultConstructorType).GetDeclaredConstructors());
    }

    [Fact]
    public void GetDeclaredPublicConstructors_OnlyFindsInstanceConstructors()
    {
        // Issue #1238 - Don't consider static constructors during DI.
        Assert.Equal(2, typeof(DeclaredConstructorType).GetDeclaredPublicConstructors().Length);
    }

    [Fact]
    public void GetDeclaredPublicConstructors_FindsDefaultInstanceConstructors()
    {
        Assert.Single(typeof(DefaultConstructorType).GetDeclaredPublicConstructors());
    }

    private class DefaultConstructorType
    {
        // No constructor declared - allows tests for default constructor detection.
    }

    private class DeclaredConstructorType
    {
        // Values here to ensure constructors get used and not
        // optimized out by the compiler.
        private static readonly Guid StaticValue;

        private readonly Guid _instanceValue;

        static DeclaredConstructorType()
        {
            StaticValue = Guid.NewGuid();
        }

        public DeclaredConstructorType()
        {
            _instanceValue = Guid.NewGuid();
        }

        public DeclaredConstructorType(string parameter)
        {
            _instanceValue = Guid.NewGuid();
        }

        protected DeclaredConstructorType(int parameter)
        {
            _instanceValue = Guid.NewGuid();
        }

        private DeclaredConstructorType(bool parameter)
        {
        }
    }

    private class DeclaredMethodType
    {
        public void PublicInstanceMethod()
        {
        }

        protected void ProtectedInstanceMethod()
        {
        }

        private void PrivateInstanceMethod()
        {
        }

        public static void PublicStaticMethod()
        {
        }

        protected static void ProtectedStaticMethod()
        {
        }

        private static void PrivateStaticMethod()
        {
        }
    }

    private class DeclaredPropertyType
    {
        public string PublicInstanceProperty { get; set; }

        protected string ProtectedInstanceProperty { get; set; }

        private string PrivateInstanceProperty { get; set; }

        public static string PublicStaticProperty { get; set; }

        protected static string ProtectedStaticProperty { get; set; }

        private static string PrivateStaticProperty { get; set; }
    }
}
