using Autofac.Core;

namespace Autofac.Benchmarks;

public class LambdaResolveBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MyDependency1>();

        // Existing method, capture the argument directly.
        builder.Register((MyDependency1 dep1) => new MyComponent(dep1, "arg"))
               .As<IServiceExistingMethodCapturedArg>();

        // New parameter-supporting method, but the argument comes from a resolve parameter.
        // Name is different just so I can run both side-by-side for now.
        builder.Register((MyDependency1 dep1, string arg1) => new MyComponent(dep1, arg1))
               .As<IServiceNewMethodOpenArg>();

        // Check against the reflection way as well.
        builder
           .RegisterType<MyComponent>()
           .UsingConstructor(typeof(MyDependency1), typeof(string))
           .WithParameter(TypedParameter.From("arg"))
           .As<IServiceFromReflection>();

        _container = builder.Build();
    }

    [Benchmark]
    public object ResolveWithCapturedParameter()
    {
        return _container.Resolve<IServiceNewMethodCapturedArg>();
    }

    [Benchmark]
    public object ResolveWithProvidedTypedParameter()
    {
        return _container.Resolve<IServiceNewMethodOpenArg>(TypedParameter.From("arg"));
    }

    [Benchmark]
    public object ResolveWithProvidedNamedParameter()
    {
        return _container.Resolve<IServiceNewMethodOpenArg>(new NamedParameter("arg1", "value"));
    }

    [Benchmark]
    public object ResolveReflectionWithRegisteredParameter()
    {
        return _container.Resolve<IServiceFromReflection>();
    }

    internal interface IServiceExistingMethodCapturedArg
    {
    }

    internal interface IServiceNewMethodCapturedArg
    {
    }

    internal interface IServiceNewMethodOpenArg
    {
    }

    internal interface IServiceFromReflection
    {
    }

    internal class MyComponent : IServiceExistingMethodCapturedArg, IServiceNewMethodCapturedArg, IServiceNewMethodOpenArg, IServiceFromReflection
    {
        private readonly MyDependency1 _dep1;
        private readonly string _str;

        public MyComponent(MyDependency1 dep1, string str)
        {
            _dep1 = dep1;
            _str = str;
        }
    }

    internal class MyDependency1
    {
    }
}
