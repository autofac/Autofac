// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Measures the first resolution of distinct implicit <c>Func</c> factory signatures, which
/// creates the generator for each signature, and the cost of calling a factory afterwards.
/// </summary>
/// <remarks>
/// <para>
/// See https://github.com/autofac/Autofac/issues/1523. A generator is created once per
/// <c>(delegateType, parameterMapping)</c> and cached in <see cref="ReflectionCacheSet.Shared"/>,
/// so <see cref="BuildContainerAndResolveFactories"/> clears those entries for its own delegate
/// types on every invocation; otherwise only the first invocation would create anything. A new
/// container is built each time because each container also keeps the factory registrations it
/// has created. <see cref="BuildContainer"/> measures the build and clear alone, for subtraction.
/// </para>
/// <para>
/// Steady-state measurement understates the saving: the JIT and expression compiler work that a
/// fresh process pays on its first factory is only paid once here. Compare against a baseline
/// package version using a command such as
/// <c>dotnet run -c Release --project bench/Autofac.Benchmarks -- --baseline-version 9.3.4
/// --filter *GeneratedFactoryFirstResolveBenchmark*</c>.
/// </para>
/// </remarks>
public class GeneratedFactoryFirstResolveBenchmark
{
    private static readonly Type[] _funcDefinitions = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>) };

    private static readonly Type[] _argumentTypes = { typeof(int), typeof(string), typeof(long) };

    private Type[] _productTypes = Array.Empty<Type>();

    private Type[] _factoryTypes = Array.Empty<Type>();

    private HashSet<Type> _factoryTypeSet = new();

    private IContainer _container = default!;

    private Func<Product<Zero>> _factory0 = default!;

    private Func<int, ProductWithArgument> _factory1 = default!;

    [Params(1, 10, 100)]
    public int SignatureCount
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        _productTypes = DistinctProductTypes(SignatureCount);

        // Cycle through zero to three arguments so every precompiled arity is represented.
        _factoryTypes = _productTypes
            .Select((product, index) => _funcDefinitions[index % 4].MakeGenericType(_argumentTypes.Take(index % 4).Append(product).ToArray()))
            .ToArray();
        _factoryTypeSet = new HashSet<Type>(_factoryTypes);

        if (_factoryTypeSet.Count != SignatureCount)
        {
            throw new InvalidOperationException("Factory signatures are not distinct.");
        }

        var builder = new ContainerBuilder();
        builder.RegisterType<Product<Zero>>();
        builder.RegisterType<ProductWithArgument>();
        _container = builder.Build();
        _factory0 = _container.Resolve<Func<Product<Zero>>>();
        _factory1 = _container.Resolve<Func<int, ProductWithArgument>>();
    }

    [GlobalCleanup]
    public void Cleanup() => _container.Dispose();

    [Benchmark]
    public void BuildContainer()
    {
        using var container = BuildProductContainer();
        GC.KeepAlive(container);
    }

    [Benchmark]
    public void BuildContainerAndResolveFactories()
    {
        using var container = BuildProductContainer();

        foreach (var factoryType in _factoryTypes)
        {
            GC.KeepAlive(container.Resolve(factoryType));
        }
    }

    [Benchmark]
    public object InvokeFactoryWithoutArguments() => _factory0();

    [Benchmark]
    public object InvokeFactoryWithArgument() => _factory1(42);

    private IContainer BuildProductContainer()
    {
        ReflectionCacheSet.Shared.Clear((member, _) => member is Type type && _factoryTypeSet.Contains(type));

        var builder = new ContainerBuilder();

        foreach (var productType in _productTypes)
        {
            builder.RegisterType(productType);
        }

        return builder.Build();
    }

    /// <summary>
    /// Produces <paramref name="count"/> distinct closed <see cref="Product{T}"/> types by
    /// encoding each index as a nesting of <see cref="Wrap{TA, TB}"/> over two markers.
    /// </summary>
    /// <param name="count">The number of distinct types to produce.</param>
    /// <returns>The distinct closed product types.</returns>
    private static Type[] DistinctProductTypes(int count)
    {
        var bits = 1;
        while (1 << bits < count)
        {
            bits++;
        }

        var types = new Type[count];

        for (var i = 0; i < count; i++)
        {
            var argument = typeof(Zero);

            for (var bit = 0; bit < bits; bit++)
            {
                argument = typeof(Wrap<,>).MakeGenericType(argument, ((i >> bit) & 1) == 1 ? typeof(One) : typeof(Zero));
            }

            types[i] = typeof(Product<>).MakeGenericType(argument);
        }

        return types;
    }

    private sealed class Zero
    {
    }

    private sealed class One
    {
    }

    private sealed class Wrap<TA, TB>
    {
    }

    private sealed class Product<T>
    {
    }

    private sealed class ProductWithArgument
    {
        public ProductWithArgument(int value)
        {
            Value = value;
        }

        public int Value
        {
            get;
        }
    }
}
