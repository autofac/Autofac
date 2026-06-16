// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Measures the cost of resolving a generated factory delegate (<c>Func&lt;T&gt;</c>) from
/// repeated child lifetime scopes where the product type <c>T</c> is registered in the root
/// container.
/// </summary>
/// <remarks>
/// See https://github.com/autofac/Autofac/issues/1461. Prior to the fix, each child scope
/// resolution caused the factory's expression tree to be recompiled via
/// <c>Expression.Lambda(...).Compile()</c>. The fix caches the compiled generator keyed on
/// <c>(delegateType, parameterMapping)</c> so the expensive compile happens once and is reused
/// across all scopes. Compare against a baseline package version using a command such as
/// <c>dotnet run -c Release --project bench/Autofac.Benchmarks -- --baseline-version 9.1.1
/// --filter *GeneratedFactoryChildScopeBenchmark*</c>.
/// </remarks>
public class GeneratedFactoryChildScopeBenchmark
{
    private IContainer _container = default!;

    [Params(100, 1000)]
    public int ScopeCount
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Product>();
        _container = builder.Build();
    }

    /// <summary>
    /// Resolves <c>Func&lt;Product&gt;</c> from a fresh child scope on each iteration.
    /// Before the fix, each child-scope resolution recompiled the factory expression tree;
    /// after the fix, the compiled generator is retrieved from a shared cache.
    /// </summary>
    [Benchmark]
    public void ResolveGeneratedFactoryFromChildScope()
    {
        for (var i = 0; i < ScopeCount; i++)
        {
            using var scope = _container.BeginLifetimeScope();
            var factory = scope.Resolve<Func<Product>>();
            GC.KeepAlive(factory());
        }
    }

    private sealed class Product
    {
    }
}
