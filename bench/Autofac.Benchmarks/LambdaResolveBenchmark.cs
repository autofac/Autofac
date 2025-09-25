// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

public class LambdaResolveBenchmark
{
    private IContainer _container;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MyDependency1>();

        // The original component context way.
        builder.Register((context) => new MyComponent(context.Resolve<MyDependency1>(), "arg"))
               .As<IServiceExistingMethodCapturedArg>();

        // Capture the argument directly.
        builder.Register((MyDependency1 dep1) => new MyComponent(dep1, "arg"))
               .As<IServiceNewMethodCapturedArg>();

        // New parameter-supporting method, but the argument comes from a resolve parameter.
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
    public object ResolveManuallyWithComponentContext()
    {
        return _container.Resolve<IServiceExistingMethodCapturedArg>();
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
