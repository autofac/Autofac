// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

public class RequiredPropertyBenchmark
{
    private readonly Parameter[] _resolveTimeParameters = { new TypedParameter(typeof(ServiceB), new ServiceB()) };

    private readonly List<Parameter> _resolveTimeParameterList = new() { new TypedParameter(typeof(ServiceB), new ServiceB()) };

    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<ConstructorComponent>();
        builder.RegisterType<RequiredPropertyComponent>();
        builder.RegisterType<ConfiguredPropertyComponent>().WithProperty(nameof(ConfiguredPropertyComponent.ServiceB), new ServiceB());
        builder.RegisterType<ResolveTimeParameterComponent>();

        _container = builder.Build();
    }

    [Benchmark(Baseline = true)]
    public void NormalConstructor()
    {
        _container.Resolve<ConstructorComponent>();
    }

    [Benchmark]
    public void RequiredProperties()
    {
        _container.Resolve<RequiredPropertyComponent>();
    }

    [Benchmark]
    public void RequiredPropertiesWithConfiguredProperty()
    {
        _container.Resolve<ConfiguredPropertyComponent>();
    }

    [Benchmark]
    public void RequiredPropertiesWithResolveTimeParameter()
    {
        _container.Resolve<ResolveTimeParameterComponent>(_resolveTimeParameters);
    }

    [Benchmark]
    public void RequiredPropertiesWithResolveTimeParameterList()
    {
        _container.Resolve<ResolveTimeParameterComponent>(_resolveTimeParameterList);
    }

    private class ServiceA
    {
    }

    private class ServiceB
    {
    }

    private class ConstructorComponent
    {
        public ConstructorComponent(ServiceA serviceA, ServiceB serviceB)
        {
            ServiceA = serviceA;
            ServiceB = serviceB;
        }

        public ServiceA ServiceA
        {
            get;
        }

        public ServiceB ServiceB
        {
            get;
        }
    }

    private class RequiredPropertyComponent
    {
        public required ServiceA ServiceA
        {
            get; set;
        }

        public required ServiceA ServiceB
        {
            get; set;
        }
    }

    private class ResolveTimeParameterComponent
    {
        public required ServiceA ServiceA
        {
            get; set;
        }

        public required ServiceB ServiceB
        {
            get; set;
        }
    }

    private class ConfiguredPropertyComponent
    {
        public required ServiceA ServiceA
        {
            get; set;
        }

        public required ServiceB ServiceB
        {
            get; set;
        }
    }
}
