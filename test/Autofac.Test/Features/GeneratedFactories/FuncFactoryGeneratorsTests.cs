// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.GeneratedFactories;

namespace Autofac.Test.Features.GeneratedFactories;

#pragma warning disable CS0618

public class FuncFactoryGeneratorsTests
{
    public static TheoryData<Type> SupportedDelegateTypes => new()
    {
        typeof(Func<Product0>),
        typeof(Func<int, Product1>),
        typeof(Func<long, string, Product2>),
        typeof(Func<int, long, string, Product3>),
        typeof(Func<Guid, long, int, string, Product4>),
    };

    public static TheoryData<Type, ParameterMapping> UnsupportedDelegates => new()
    {
        // More arguments than the precompiled generators cover.
        { typeof(Func<int, long, string, Guid, char, Product5>), ParameterMapping.ByType },

        // Duplicate argument types must keep failing when the factory is invoked (#269).
        { typeof(Func<int, int, string, Product3>), ParameterMapping.ByType },

        // Custom delegates and non-type mappings keep the compiled expression path.
        { typeof(Product1.Factory), ParameterMapping.ByType },
        { typeof(Func<int, Product1>), ParameterMapping.ByName },
        { typeof(Func<int, Product1>), ParameterMapping.ByPosition },
#if NET9_0_OR_GREATER

        // Ref struct arguments cannot close the precompiled generic generators.
        { typeof(Func<ReadOnlySpan<char>, Product1>), ParameterMapping.ByType },
#endif
    };

    [Theory]
    [MemberData(nameof(SupportedDelegateTypes))]
    public void TypeMappedFuncsUsePrecompiledGenerators(Type delegateType)
    {
        Assert.NotNull(FuncFactoryGenerators.TryCreateServiceGenerator(delegateType, ParameterMapping.ByType));
        Assert.NotNull(FuncFactoryGenerators.TryCreateRegistrationGenerator(delegateType, ParameterMapping.ByType));
    }

    [Theory]
    [MemberData(nameof(UnsupportedDelegates))]
    public void OtherDelegatesUseCompiledExpressions(Type delegateType, ParameterMapping parameterMapping)
    {
        Assert.Null(FuncFactoryGenerators.TryCreateServiceGenerator(delegateType, parameterMapping));
        Assert.Null(FuncFactoryGenerators.TryCreateRegistrationGenerator(delegateType, parameterMapping));
    }

    [Fact]
    public void ImplicitFuncsMapArgumentsByType()
    {
        var builder = new ContainerBuilder();
        RegisterProducts(builder);
        using var container = builder.Build();

        AssertProductsMapArgumentsByType(
            container.Resolve<Func<Product0>>(),
            container.Resolve<Func<int, Product1>>(),
            container.Resolve<Func<long, string, Product2>>(),
            container.Resolve<Func<int, long, string, Product3>>(),
            container.Resolve<Func<Guid, long, int, string, Product4>>());
    }

    [Fact]
    public void RegisteredGeneratedFuncsMapArgumentsByType()
    {
        var builder = new ContainerBuilder();
        RegisterProducts(builder);
        builder.RegisterGeneratedFactory<Func<Product0>>();
        builder.RegisterGeneratedFactory<Func<int, Product1>>();
        builder.RegisterGeneratedFactory<Func<long, string, Product2>>();
        builder.RegisterGeneratedFactory<Func<int, long, string, Product3>>();
        builder.RegisterGeneratedFactory<Func<Guid, long, int, string, Product4>>();
        using var container = builder.Build();

        AssertProductsMapArgumentsByType(
            container.Resolve<Func<Product0>>(),
            container.Resolve<Func<int, Product1>>(),
            container.Resolve<Func<long, string, Product2>>(),
            container.Resolve<Func<int, long, string, Product3>>(),
            container.Resolve<Func<Guid, long, int, string, Product4>>());
    }

    [Fact]
    public void FuncsPassNullArguments()
    {
        var builder = new ContainerBuilder();
        RegisterProducts(builder);
        using var container = builder.Build();

        var product = container.Resolve<Func<long, string, Product2>>()(3, null!);

        Assert.Equal(3, product.Size);
        Assert.Null(product.Text);
    }

    [Fact]
    public void FuncsResolveFromTheirOwnScope()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Product1>().InstancePerLifetimeScope();
        using var container = builder.Build();
        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var factory1 = scope1.Resolve<Func<int, Product1>>();
        var factory2 = scope2.Resolve<Func<int, Product1>>();

        Assert.Same(factory1(1), factory1(2));
        Assert.NotSame(factory1(1), factory2(1));
        Assert.Same(scope1.Resolve<Product1>(), factory1(3));
    }

    [Fact]
    public void FuncsBeyondFourArgumentsStillMapArgumentsByType()
    {
        var id = Guid.NewGuid();
        var builder = new ContainerBuilder();
        builder.RegisterType<Product5>();
        using var container = builder.Build();

        var factory = container.Resolve<Func<int, long, string, Guid, char, Product5>>();
        var product = factory(1, 2, "three", id, 'v');

        AssertCompiledExpression(factory);
        Assert.Equal((1, 2L, "three", id, 'v'), (product.Number, product.Size, product.Text, product.Id, product.Letter));
    }

    private static void RegisterProducts(ContainerBuilder builder)
    {
        builder.RegisterType<Product0>();
        builder.RegisterType<Product1>();
        builder.RegisterType<Product2>();
        builder.RegisterType<Product3>();
        builder.RegisterType<Product4>();
    }

    private static void AssertProductsMapArgumentsByType(
        Func<Product0> factory0,
        Func<int, Product1> factory1,
        Func<long, string, Product2> factory2,
        Func<int, long, string, Product3> factory3,
        Func<Guid, long, int, string, Product4> factory4)
    {
        var id = Guid.NewGuid();

        Assert.IsType<Product0>(factory0());
        Assert.Equal(1, factory1(1).Number);

        var product2 = factory2(2, "two");
        Assert.Equal((2L, "two"), (product2.Size, product2.Text));

        var product3 = factory3(3, 33, "three");
        Assert.Equal((3, 33L, "three"), (product3.Number, product3.Size, product3.Text));

        // The constructor takes its parameters in a different order from the Func arguments.
        var product4 = factory4(id, 44, 4, "four");
        Assert.Equal((4, 44L, "four", id), (product4.Number, product4.Size, product4.Text, product4.Id));

        AssertPrecompiled(factory0);
        AssertPrecompiled(factory1);
        AssertPrecompiled(factory2);
        AssertPrecompiled(factory3);
        AssertPrecompiled(factory4);
    }

    // A compiled expression is a DynamicMethod, which has no declaring type.
    private static void AssertPrecompiled(Delegate factory)
        => Assert.NotNull(factory.Method.DeclaringType);

    private static void AssertCompiledExpression(Delegate factory)
        => Assert.Null(factory.Method.DeclaringType);

    public class Product0
    {
    }

    public class Product1
    {
        public Product1(int value)
        {
            Number = value;
        }

        public delegate Product1 Factory(int value);

        public int Number
        {
            get;
        }
    }

    public class Product2
    {
        public Product2(string text, long value)
        {
            Text = text;
            Size = value;
        }

        public string Text
        {
            get;
        }

        public long Size
        {
            get;
        }
    }

    public class Product3
    {
        public Product3(string text, long longValue, int intValue)
        {
            Text = text;
            Size = longValue;
            Number = intValue;
        }

        public string Text
        {
            get;
        }

        public long Size
        {
            get;
        }

        public int Number
        {
            get;
        }
    }

    public class Product4
    {
        public Product4(string text, int intValue, long longValue, Guid id)
        {
            Text = text;
            Number = intValue;
            Size = longValue;
            Id = id;
        }

        public string Text
        {
            get;
        }

        public int Number
        {
            get;
        }

        public long Size
        {
            get;
        }

        public Guid Id
        {
            get;
        }
    }

    public class Product5
    {
        public Product5(char character, Guid id, string text, long longValue, int intValue)
        {
            Letter = character;
            Id = id;
            Text = text;
            Size = longValue;
            Number = intValue;
        }

        public char Letter
        {
            get;
        }

        public Guid Id
        {
            get;
        }

        public string Text
        {
            get;
        }

        public long Size
        {
            get;
        }

        public int Number
        {
            get;
        }
    }
}
