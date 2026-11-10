// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators.Scenario;
using Autofac.Core;

namespace Autofac.Benchmarks.Decorators;

/// <summary>
/// Benchmarks resolving a decorated service with a resolve-time parameter, passed as an array and as a list.
/// </summary>
public class KeylessParameterBenchmark
{
    private readonly Parameter[] _parameterArray = { new NamedParameter("unused", 1) };
    private readonly List<Parameter> _parameterList = new() { new NamedParameter("unused", 1) };
    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<CommandHandlerOne>().As<ICommandHandler>();
        builder.RegisterDecorator<CommandHandlerDecoratorOne, ICommandHandler>();
        _container = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public object ResolveWithParameterArray() => _container.Resolve<ICommandHandler>(_parameterArray);

    [Benchmark]
    public object ResolveWithParameterList() => _container.Resolve<ICommandHandler>(_parameterList);

    [GlobalCleanup]
    public void Cleanup() => _container.Dispose();
}
