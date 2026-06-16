// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Features.GeneratedFactories;

#pragma warning disable CS0618

public class GeneratedFactoriesTests
{
    private class A<T>
    {
        public T P
        {
            get; private set;
        }

        public delegate A<T> Factory(T p);

        public A(T p)
        {
            P = p;
        }
    }

    [Fact]
    public void CreateGenericFromFactoryDelegate()
    {
        var cb = new ContainerBuilder();

        cb.RegisterType<A<string>>();
        cb.RegisterGeneratedFactory<A<string>.Factory>(new TypedService(typeof(A<string>)));

        var container = cb.Build();

        var factory = container.Resolve<A<string>.Factory>();
        Assert.NotNull(factory);

        var s = "Hello!";
        var a = factory(s);
        Assert.NotNull(a);
        Assert.Equal(s, a.P);
    }

    [Fact]
    public void CreateGenericFromFactoryDelegateImpliedServiceType()
    {
        var cb = new ContainerBuilder();

        cb.RegisterType<A<string>>();
        cb.RegisterGeneratedFactory<A<string>.Factory>();

        var container = cb.Build();

        var factory = container.Resolve<A<string>.Factory>();
        Assert.NotNull(factory);

        var s = "Hello!";
        var a = factory(s);
        Assert.NotNull(a);
        Assert.Equal(s, a.P);
    }

    private class QuoteService
    {
        public decimal GetQuote(string symbol)
        {
            return 2m;
        }
    }

    private class Shareholding
    {
        public delegate Shareholding Factory(string symbol, uint holding);

        public Shareholding(string symbol, uint holding, QuoteService qs)
        {
            Symbol = symbol;
            Holding = holding;
            _qs = qs;
        }

        private readonly QuoteService _qs;

        public string Symbol
        {
            get; private set;
        }

        public uint Holding
        {
            get; set;
        }

        public decimal Quote()
        {
            return _qs.GetQuote(Symbol) * Holding;
        }
    }

    [Fact]
    public void ShareholdingExample()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<QuoteService>();

        builder.RegisterType<Shareholding>();

        builder.RegisterGeneratedFactory<Shareholding.Factory>(
            new TypedService(typeof(Shareholding)));

        var container = builder.Build();

        var shareholdingFactory = container.Resolve<Shareholding.Factory>();

        var shareholding = shareholdingFactory.Invoke("ABC", 1234);

        Assert.Equal("ABC", shareholding.Symbol);
        Assert.Equal<uint>(1234, shareholding.Holding);
        Assert.Equal(1234m * 2, shareholding.Quote());
    }

    [Fact]
    public void ShareholdingExampleMatchingFuncParametersByType()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<QuoteService>();

        builder.RegisterType<Shareholding>();

        builder.RegisterGeneratedFactory<Func<string, uint, Shareholding>>(
            new TypedService(typeof(Shareholding)));

        var container = builder.Build();

        var shareholdingFactory = container.Resolve<Func<string, uint, Shareholding>>();

        var shareholding = shareholdingFactory.Invoke("ABC", 1234);

        Assert.Equal("ABC", shareholding.Symbol);
        Assert.Equal<uint>(1234, shareholding.Holding);
        Assert.Equal(1234m * 2, shareholding.Quote());
    }

    private class StringHolder
    {
        public delegate StringHolder Factory();

        public string S
        {
            get; set;
        }
    }

    [Fact]
    public void RespectsContexts()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<StringHolder>()
            .InstancePerLifetimeScope();
        builder.RegisterGeneratedFactory<StringHolder.Factory>(new TypedService(typeof(StringHolder)));

        var container = builder.Build();

        container.Resolve<StringHolder>().S = "Outer";
        var inner = container.BeginLifetimeScope();
        inner.Resolve<StringHolder>().S = "Inner";

        var outerFac = container.Resolve<StringHolder.Factory>();
        Assert.Equal("Outer", outerFac().S);

        var innerFac = inner.Resolve<StringHolder.Factory>();
        Assert.Equal("Inner", innerFac().S);
    }

    [Fact]
    public void CanSetParameterMappingToPositional()
    {
        var builder = new ContainerBuilder();

        int i0 = 32, i0Actual = 0, i1 = 67, i1Actual = 0;

        builder.Register((c, p) =>
        {
            i0Actual = p.Positional<int>(0);
            i1Actual = p.Positional<int>(1);
            return new object();
        });

        builder.RegisterGeneratedFactory<Func<int, int, object>>()
            .PositionalParameterMapping();

        var container = builder.Build();

        var generated = container.Resolve<Func<int, int, object>>();

        generated(i0, i1);

        Assert.Equal(i0, i0Actual);
        Assert.Equal(i1, i1Actual);
    }

    [Fact]
    public void CanNameGeneratedFactories()
    {
        var builder = new ContainerBuilder();

        builder.RegisterGeneratedFactory<Func<object>>().Named<Func<object>>("object-factory");

        var container = builder.Build();

        var of = container.ResolveNamed<Func<object>>("object-factory");

        Assert.NotNull(of);
    }

    // This became necessary because by default the char[] constructor of string
    // is chosen in the presence of implicit collection support.
    private class HasCharIntCtor
    {
        public string Str
        {
            get; private set;
        }

        public HasCharIntCtor(char c, int i)
        {
            Str = new string(c, i);
        }
    }

    [Fact]
    public void CanAutoGenerateFactoryFromFunc()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<HasCharIntCtor>();

        var container = builder.Build();

        var sf = container.Resolve<Func<char, int, HasCharIntCtor>>();
        var str = sf('a', 3).Str;

        Assert.Equal("aaa", str);
    }

    public delegate string CharCountStringFactory(char c, int count);

    [Fact]
    public void CanAutoGenerateFactoriesFromCustomDelegateTypes()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<string>();

        var container = builder.Build();

        var sf = container.Resolve<CharCountStringFactory>();
        var str = sf('a', 3);

        Assert.Equal("aaa", str);
    }

    [Fact]
    public void WillNotAutoGenerateFactoriesWhenProductNotRegistered()
    {
        var builder = new ContainerBuilder();

        var container = builder.Build();

        Assert.False(container.IsRegistered<Func<char, int, string>>());
    }

    [Fact]
    public void CreateGenericFromNonGenericFactoryDelegate()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<A<string>>();
        builder.RegisterGeneratedFactory(typeof(A<string>.Factory), new TypedService(typeof(A<string>)));

        var container = builder.Build();

        var factory = container.Resolve<A<string>.Factory>();
        Assert.NotNull(factory);

        var s = "Hello!";
        var a = factory(s);
        Assert.NotNull(a);
        Assert.Equal(s, a.P);
    }

    [Fact]
    public void CreateGenericFromNonGenericFactoryDelegateImpliedServiceType()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<A<string>>();
        builder.RegisterGeneratedFactory(typeof(A<string>.Factory));

        var container = builder.Build();

        var factory = container.Resolve<A<string>.Factory>();
        Assert.NotNull(factory);

        var s = "Hello!";
        var a = factory(s);
        Assert.NotNull(a);
        Assert.Equal(s, a.P);
    }

    [Fact]
    public void WhenMultipleProductsAreRegistered_MultipleFactoriesCanBeResolved()
    {
        var o1 = new object();
        var o2 = new object();

        var builder = new ContainerBuilder();
        builder.RegisterInstance(o1);
        builder.RegisterInstance(o2);
        var container = builder.Build();

        var factories = container.Resolve<IEnumerable<Func<object>>>();

        Assert.Equal(2, factories.Count());
        Assert.Contains(factories, f => f() == o1);
        Assert.Contains(factories, f => f() == o2);
    }

    [Fact]
    public void ResolvingAutoGeneratedFactoryByName_ReturnsProductsByName()
    {
        var o = new object();

        var builder = new ContainerBuilder();
        builder.RegisterInstance(o).Named<object>("o");
        var container = builder.Build();

        var fac = container.ResolveNamed<Func<object>>("o");

        Assert.Same(o, fac());
    }

    [Fact]
    public void DuplicateConstructorParameterTypesDoNotResolve()
    {
        // Issue #269: An object with duplicate constructor parameter types should fail Func resolution from an auto-generated factory.
        var builder = new ContainerBuilder();
        builder.RegisterType<DuplicateConstructorParameterTypes>();
        var container = builder.Build();
        var func = container.Resolve<Func<int, int, string, DuplicateConstructorParameterTypes>>();
        Assert.Throws<DependencyResolutionException>(() => func(1, 2, "3"));
    }

    [Fact]
    public void FactoryOverrideCanBeRegisteredForDuplicateParameterTypes()
    {
        // Issue #269: If you register a custom Func for an object with duplicate parameters it should override the auto-generated factory.
        var builder = new ContainerBuilder();
        builder.RegisterType<DuplicateConstructorParameterTypes>();
        builder.Register<Func<int, int, string, DuplicateConstructorParameterTypes>>(ctx => (a, b, c) => new DuplicateConstructorParameterTypes(a, b, c));
        var container = builder.Build();
        var func = container.Resolve<Func<int, int, string, DuplicateConstructorParameterTypes>>();
        var obj = func(1, 2, "3");
        Assert.Equal(1, obj.A);
        Assert.Equal(2, obj.B);
        Assert.Equal("3", obj.C);
    }

    [Fact]
    public void SpecificDelegateCanBeRegisteredForDuplicateParameterTypes()
    {
        // Issue #269: If you register a specific delegate type for an object with duplicate parameters it should work and not throw an exception.
        var builder = new ContainerBuilder();
        builder.RegisterType<DuplicateConstructorParameterTypes>();
        builder.RegisterGeneratedFactory<DuplicateConstructorParameterTypes.Factory>(new TypedService(typeof(DuplicateConstructorParameterTypes)));
        var container = builder.Build();
        var func = container.Resolve<DuplicateConstructorParameterTypes.Factory>();
        var obj = func(1, 2, "3");
        Assert.Equal(1, obj.A);
        Assert.Equal(2, obj.B);
        Assert.Equal("3", obj.C);
    }

    private class DuplicateConstructorParameterTypes
    {
        public delegate DuplicateConstructorParameterTypes Factory(int a, int b, string c);

        public int A
        {
            get; set;
        }

        public int B
        {
            get; set;
        }

        public string C
        {
            get; set;
        }

        // This constructor should not be able to be resolved into a Func<int, int, string, DuplicateConstructorParameterTypes>
        // because of the redundant types in the constructor.
        public DuplicateConstructorParameterTypes(int a, int b, string c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    // #1461 — types used by the regression tests below.
    private class Dependency1461
    {
    }

    private class DependencyWithParam1461
    {
        public string Value
        {
            get;
        }

        public DependencyWithParam1461(string value)
        {
            Value = value;
        }
    }

    private record FuncConsumer1461(Func<Dependency1461> Factory);

    [Fact]
    public void LifetimeWithConsumer_SiblingScopes()
    {
        // #1461: Resolving Func<T> from sibling child scopes where T is registered in the
        // parent must not recompile the expression tree for each scope. The factory
        // delegates produced in each scope must resolve the correct product type.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>();
        using var container = builder.Build();

        using var lifetimeScope1 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        using var lifetimeScope2 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        using var lifetimeScope3 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());

        var c1 = lifetimeScope1.Resolve<FuncConsumer1461>();
        var c2 = lifetimeScope2.Resolve<FuncConsumer1461>();
        var c3 = lifetimeScope3.Resolve<FuncConsumer1461>();

        Assert.NotNull(c1.Factory());
        Assert.NotNull(c2.Factory());
        Assert.NotNull(c3.Factory());
        Assert.IsType<Dependency1461>(c1.Factory());
        Assert.IsType<Dependency1461>(c2.Factory());
        Assert.IsType<Dependency1461>(c3.Factory());
    }

    [Fact]
    public void FactoryDelegate_CompiledGeneratorCached_SiblingScopes()
    {
        // #1461: The compiled expression-tree generator for a given (delegateType, parameterMapping)
        // pair must be compiled exactly once and reused across sibling child scopes.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>();
        using var container = builder.Build();

        var cacheKey = (typeof(Func<Dependency1461>), Autofac.Features.GeneratedFactories.ParameterMapping.ByType);

        using var scope1 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        scope1.Resolve<FuncConsumer1461>();

        // After the first resolve, the compiled generator must be present in the cache.
        Assert.True(ReflectionCacheSet.Shared.Internal.GeneratedFactoryServiceRegistrationGenerators.ContainsKey(cacheKey));

        var countAfterFirstScope = ReflectionCacheSet.Shared.Internal.GeneratedFactoryServiceRegistrationGenerators.Count;

        using var scope2 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        scope2.Resolve<FuncConsumer1461>();

        using var scope3 = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        scope3.Resolve<FuncConsumer1461>();

        // Resolving the same Func<T> from additional sibling scopes must not add new cache entries;
        // the count must remain stable after the first scope warms the entry.
        var countAfterAllScopes = ReflectionCacheSet.Shared.Internal.GeneratedFactoryServiceRegistrationGenerators.Count;
        Assert.Equal(countAfterFirstScope, countAfterAllScopes);
    }

    [Fact]
    public void FactoryDelegate_WithTypedParameter_SiblingScopes()
    {
        // #1461: Caching must not break Func<TParam, TProduct> factories where parameters
        // are mapped by type (ByType). Each sibling scope must produce correct instances.
        var builder = new ContainerBuilder();
        builder.RegisterType<DependencyWithParam1461>();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<string, DependencyWithParam1461>>();
        var f2 = scope2.Resolve<Func<string, DependencyWithParam1461>>();

        Assert.Equal("hello", f1("hello").Value);
        Assert.Equal("world", f2("world").Value);
    }

    [Fact]
    public void FactoryDelegate_WithNamedParameter_SiblingScopes()
    {
        // #1461: Caching must not break custom delegate factories where parameters are mapped
        // by name (ByName). Each sibling scope must produce correct instances.
        var builder = new ContainerBuilder();
        builder.RegisterType<DependencyWithParam1461>();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<string, DependencyWithParam1461>>();
        var f2 = scope2.Resolve<Func<string, DependencyWithParam1461>>();

        Assert.Equal("a", f1("a").Value);
        Assert.Equal("b", f2("b").Value);
    }

    [Fact]
    public void FactoryDelegate_DifferentProductTypes_NoCacheCollision()
    {
        // #1461: The cache must not collide across different product types.
        // Func<Dependency1461> and Func<DependencyWithParam1461> must each compile their
        // own generator entry and produce the correct product type.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>();
        builder.RegisterType<DependencyWithParam1461>();
        using var container = builder.Build();

        using var scope = container.BeginLifetimeScope();

        var f1 = scope.Resolve<Func<Dependency1461>>();
        var f2 = scope.Resolve<Func<string, DependencyWithParam1461>>();

        Assert.IsType<Dependency1461>(f1());
        Assert.IsType<DependencyWithParam1461>(f2("test"));
        Assert.Equal("test", f2("test").Value);
    }

    [Fact]
    public void FactoryDelegate_InstancePerDependency_SiblingScopes()
    {
        // #1461: When the product is InstancePerDependency, each factory invocation must
        // produce a distinct instance even when the compiled generator is shared.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>().InstancePerDependency();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<Dependency1461>>();
        var f2 = scope2.Resolve<Func<Dependency1461>>();

        Assert.NotSame(f1(), f1());
        Assert.NotSame(f2(), f2());
        Assert.NotSame(f1(), f2());
    }

    [Fact]
    public void FactoryDelegate_InstancePerLifetimeScope_SiblingScopes()
    {
        // #1461: When the product is InstancePerLifetimeScope, the factory must return the
        // same instance within a scope but a different instance across sibling scopes.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>().InstancePerLifetimeScope();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<Dependency1461>>();
        var f2 = scope2.Resolve<Func<Dependency1461>>();

        Assert.Same(f1(), f1());
        Assert.Same(f2(), f2());
        Assert.NotSame(f1(), f2());
    }

    [Fact]
    public void FactoryDelegate_SingleInstance_SiblingScopes()
    {
        // #1461: When the product is SingleInstance, every factory invocation across all
        // scopes must return the same singleton — the cached compiled generator must not
        // change singleton semantics.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>().SingleInstance();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<Dependency1461>>();
        var f2 = scope2.Resolve<Func<Dependency1461>>();

        Assert.Same(f1(), f2());
    }

    [Fact]
    public void FactoryDelegate_RespectsChildScopeContext_SiblingScopes()
    {
        // #1461: Each factory delegate must resolve from its own child scope, not a shared
        // ancestor scope. InstancePerLifetimeScope behaviour validates this.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>().InstancePerLifetimeScope();
        using var container = builder.Build();

        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var f1 = scope1.Resolve<Func<Dependency1461>>();
        var f2 = scope2.Resolve<Func<Dependency1461>>();

        var result1 = f1();
        var result2 = f2();

        Assert.NotNull(result1);
        Assert.NotNull(result2);

        // Each factory is bound to its own scope — instances must differ.
        Assert.NotSame(result1, result2);

        // Within the same scope, calling the factory twice returns the same instance.
        Assert.Same(result1, f1());
        Assert.Same(result2, f2());
    }

    [Fact]
    public void FactoryDelegate_CompiledGeneratorCache_ClearedByReflectionCacheSetClear()
    {
        // #1461: The generated-factory caches must participate in ReflectionCacheSet.Clear()
        // so that types from collectible AssemblyLoadContexts can be fully unloaded.
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency1461>();
        using var container = builder.Build();

        using var scope = container.BeginLifetimeScope(c => c.RegisterType<FuncConsumer1461>());
        scope.Resolve<FuncConsumer1461>();

        var serviceRegCache = ReflectionCacheSet.Shared.Internal.GeneratedFactoryServiceRegistrationGenerators;

        Assert.NotEmpty(serviceRegCache);

        ReflectionCacheSet.Shared.Clear();

        Assert.Empty(serviceRegCache);
    }
}
