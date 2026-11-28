// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Benchmarks;

/// <summary>
/// Measures calling an existing <c>Func</c> factory with zero to four arguments.
/// </summary>
/// <remarks>
/// See https://github.com/autofac/Autofac/issues/1523. Each argument count uses its own
/// precompiled generator, and the implicit and registered sources use different generators, so
/// both are measured.
/// </remarks>
public class GeneratedFactoryInvokeBenchmark
{
    private IContainer _container = default!;

    private Func<Product> _factory0 = default!;

    private Func<int, Product> _factory1 = default!;

    private Func<int, string, Product> _factory2 = default!;

    private Func<int, string, long, Product> _factory3 = default!;

    private Func<int, string, long, Guid, Product> _factory4 = default!;

    [Params(GeneratedFactorySource.Implicit, GeneratedFactorySource.Registered)]
    public GeneratedFactorySource Source
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Product>();

        if (Source == GeneratedFactorySource.Registered)
        {
#pragma warning disable CS0618 // RegisterGeneratedFactory is obsolete, but it still uses the ResolveService generators measured here.
            builder.RegisterGeneratedFactory<Func<Product>>();
            builder.RegisterGeneratedFactory<Func<int, Product>>();
            builder.RegisterGeneratedFactory<Func<int, string, Product>>();
            builder.RegisterGeneratedFactory<Func<int, string, long, Product>>();
            builder.RegisterGeneratedFactory<Func<int, string, long, Guid, Product>>();
#pragma warning restore CS0618
        }

        _container = builder.Build();
        _factory0 = _container.Resolve<Func<Product>>();
        _factory1 = _container.Resolve<Func<int, Product>>();
        _factory2 = _container.Resolve<Func<int, string, Product>>();
        _factory3 = _container.Resolve<Func<int, string, long, Product>>();
        _factory4 = _container.Resolve<Func<int, string, long, Guid, Product>>();
    }

    [GlobalCleanup]
    public void Cleanup() => _container.Dispose();

    [Benchmark]
    public object InvokeFactoryWithoutArguments() => _factory0();

    [Benchmark]
    public object InvokeFactoryWithOneArgument() => _factory1(1);

    [Benchmark]
    public object InvokeFactoryWithTwoArguments() => _factory2(1, "two");

    [Benchmark]
    public object InvokeFactoryWithThreeArguments() => _factory3(1, "two", 3);

    [Benchmark]
    public object InvokeFactoryWithFourArguments() => _factory4(1, "two", 3, Guid.Empty);

    private sealed class Product
    {
        public Product(int number = 0, string text = "", long size = 0, Guid id = default)
        {
            Number = number;
            Text = text;
            Size = size;
            Id = id;
        }

        public int Number
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

        public Guid Id
        {
            get;
        }
    }
}
