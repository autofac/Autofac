namespace Autofac.Benchmarks;

public class OpenGenericBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>));
        _container = builder.Build();
    }

    [Benchmark]
    public IService<string> ResolveOpenGeneric() => _container.Resolve<IService<string>>();

    public interface IService<T>
    {
    }

    public class Service<T> : IService<T>
    {
    }
}
