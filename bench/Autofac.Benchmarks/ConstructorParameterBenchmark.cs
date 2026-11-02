// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Benchmarks;

/// <summary>
/// Binds reflection-activated constructors whose arguments are not all autowired, or are autowired in a less common way.
/// </summary>
public class ConstructorParameterBenchmark
{
    private readonly Parameter[] _resolveTimeParameters = { new NamedParameter("value", 1) };

    private IContainer _container = default!;

    private interface IPart
    {
    }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<D1>();
        builder.RegisterType<D2>();
        builder.RegisterType<D3>();
        builder.RegisterType<D4>();
        builder.RegisterType<D5>();
        builder.RegisterType<D6>();

        builder.RegisterType<NamedArgument>().WithParameter("value", 1);
        builder.RegisterType<PositionalArgument>().WithParameter(new PositionalParameter(1, 1));
        builder.RegisterType<ResolvedArgument>().WithParameter((pi, c) => pi.Name == "value", (pi, c) => 1);
        builder.RegisterType<DefaultValueArgument>();
        builder.RegisterType<ResolveTimeArgument>();
        builder.RegisterType<CustomAutowiredArguments>().WithParameter(new CustomAutowiringParameter());
        builder.RegisterType<SixArguments>();

        builder.RegisterType<Part1>().Keyed<IPart>("key");
        builder.RegisterType<Part2>().Keyed<IPart>("key");
        builder.RegisterComposite<CompositePart, IPart>().Keyed<IPart>("key");

        _container = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public void SixAutowiredArgs()
    {
        _container.Resolve<SixArguments>();
    }

    [Benchmark]
    public void NamedParameter()
    {
        _container.Resolve<NamedArgument>();
    }

    [Benchmark]
    public void PositionalParameter()
    {
        _container.Resolve<PositionalArgument>();
    }

    [Benchmark]
    public void ResolvedParameter()
    {
        _container.Resolve<ResolvedArgument>();
    }

    [Benchmark]
    public void DefaultValueParameter()
    {
        _container.Resolve<DefaultValueArgument>();
    }

    [Benchmark]
    public void ResolveTimeParameter()
    {
        _container.Resolve<ResolveTimeArgument>(_resolveTimeParameters);
    }

    [Benchmark]
    public void AutowiringParameterSubclass()
    {
        _container.Resolve<CustomAutowiredArguments>();
    }

    [Benchmark]
    public void KeyedCompositeEnumerable()
    {
        _container.ResolveKeyed<IPart>("key");
    }

    // Not the exact AutowiringParameter type, so the binder asks it through CanSupplyValue.
    private class CustomAutowiringParameter : AutowiringParameter
    {
    }

    private class NamedArgument
    {
        public NamedArgument(D1 d1, int value)
        {
        }
    }

    private class PositionalArgument
    {
        public PositionalArgument(D1 d1, int value)
        {
        }
    }

    private class ResolvedArgument
    {
        public ResolvedArgument(D1 d1, int value)
        {
        }
    }

    private class DefaultValueArgument
    {
        public DefaultValueArgument(D1 d1, int value = 1)
        {
        }
    }

    private class ResolveTimeArgument
    {
        public ResolveTimeArgument(D1 d1, int value)
        {
        }
    }

    private class CustomAutowiredArguments
    {
        public CustomAutowiredArguments(D1 d1, D2 d2)
        {
        }
    }

    private class SixArguments
    {
        public SixArguments(D1 d1, D2 d2, D3 d3, D4 d4, D5 d5, D6 d6)
        {
        }
    }

    private class Part1 : IPart
    {
    }

    private class Part2 : IPart
    {
    }

    private class CompositePart : IPart
    {
        public CompositePart(IEnumerable<IPart> parts)
        {
        }
    }

    private class D1
    {
    }

    private class D2
    {
    }

    private class D3
    {
    }

    private class D4
    {
    }

    private class D5
    {
    }

    private class D6
    {
    }
}
