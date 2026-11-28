// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Measures the first resolution of distinct <c>Func</c> factory signatures, which creates the
/// generator for each signature.
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
    private static readonly Type[] FuncDefinitions = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

    private static readonly Type[] ArgumentTypes = { typeof(int), typeof(string), typeof(long), typeof(Guid) };

    private Type[] _productTypes = Array.Empty<Type>();

    private Type[] _factoryTypes = Array.Empty<Type>();

    private HashSet<Type> _factoryTypeSet = new();

    [Params(1, 10, 100)]
    public int SignatureCount
    {
        get; set;
    }

    [Params(GeneratedFactorySource.Implicit, GeneratedFactorySource.Registered)]
    public GeneratedFactorySource Source
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        _productTypes = DistinctProductTypes(SignatureCount);

        // Cycle through zero to four arguments so every precompiled arity is represented.
        _factoryTypes = _productTypes
            .Select((product, index) => FuncDefinitions[index % 5].MakeGenericType(ArgumentTypes.Take(index % 5).Append(product).ToArray()))
            .ToArray();
        _factoryTypeSet = new HashSet<Type>(_factoryTypes);

        if (_factoryTypeSet.Count != SignatureCount)
        {
            throw new InvalidOperationException("Factory signatures are not distinct.");
        }
    }

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

    private IContainer BuildProductContainer()
    {
        ReflectionCacheSet.Shared.Clear((member, _) => member is Type type && _factoryTypeSet.Contains(type));

        var builder = new ContainerBuilder();

        foreach (var productType in _productTypes)
        {
            builder.RegisterType(productType);
        }

        if (Source == GeneratedFactorySource.Registered)
        {
            foreach (var factoryType in _factoryTypes)
            {
#pragma warning disable CS0618 // RegisterGeneratedFactory is obsolete, but it still uses the ResolveService generators measured here.
                builder.RegisterGeneratedFactory(factoryType);
#pragma warning restore CS0618
            }
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
}
