// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

public class RequiredPropertyBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<ConstructorComponent>();
        builder.RegisterType<RequiredPropertyComponent>();

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

        public ServiceA ServiceA { get; }

        public ServiceB ServiceB { get; }
    }

    private class RequiredPropertyComponent
    {
        public required ServiceA ServiceA { get; set; }

        public required ServiceA ServiceB { get; set; }
    }
}
