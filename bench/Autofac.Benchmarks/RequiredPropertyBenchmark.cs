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

    public class ServiceA
    {
    }

    public class ServiceB
    {
    }

    public class ConstructorComponent
    {
        public ConstructorComponent(ServiceA serviceA, ServiceB serviceB)
        {
            ServiceA = serviceA;
            ServiceB = serviceB;
        }

        public ServiceA ServiceA { get; }

        public ServiceB ServiceB { get; }
    }

    public class RequiredPropertyComponent
    {
        required public ServiceA ServiceA { get; set; }

        required public ServiceA ServiceB { get; set; }
    }
}
