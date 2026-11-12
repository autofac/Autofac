// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

public class PropertyInjectionBenchmark
{
    private IContainer _container = default!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<A>().PropertiesAutowired();
        builder.RegisterType<B1>().PropertiesAutowired();
        builder.RegisterType<B2>().PropertiesAutowired();
        builder.RegisterType<C1>().PropertiesAutowired();
        builder.RegisterType<C2>().PropertiesAutowired();
        builder.RegisterType<D1>();
        builder.RegisterType<D2>();
        builder.RegisterType<PreservingSetValues>().PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);
        builder.RegisterType<DelegateSelector>().PropertiesAutowired((property, instance) => true);
        builder.RegisterType<AllowingCircularDependencies>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        builder.RegisterType<UnregisteredProperties>().PropertiesAutowired();
        _container = builder.Build();
    }

    [Benchmark]
    public void Resolve()
    {
        var instance = _container.Resolve<A>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void ResolvePreservingSetValues()
    {
        var instance = _container.Resolve<PreservingSetValues>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void ResolveWithDelegateSelector()
    {
        var instance = _container.Resolve<DelegateSelector>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void ResolveAllowingCircularDependencies()
    {
        var instance = _container.Resolve<AllowingCircularDependencies>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void ResolveWithUnregisteredProperties()
    {
        var instance = _container.Resolve<UnregisteredProperties>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void InjectPropertiesWithParameter()
    {
        var instance = _container.InjectProperties(new DelegateSelector(), TypedParameter.From(new D1()));
        GC.KeepAlive(instance);
    }

    internal class A
    {
        public B1? B1
        {
            get; set;
        }

        public B2? B2
        {
            get; set;
        }
    }

    internal class B1
    {
        public C1? C1
        {
            get; set;
        }
    }

    internal class B2
    {
        public C2? C2
        {
            get; set;
        }
    }

    internal class C1
    {
        public D1? D1
        {
            get; set;
        }
    }

    internal class C2
    {
        public D2? D2
        {
            get; set;
        }
    }

    internal class D1
    {
    }

    internal class D2
    {
    }

    internal class PreservingSetValues
    {
        public D1? D1
        {
            get; set;
        }

        public D2? D2
        {
            get; set;
        }
    }

    internal class DelegateSelector
    {
        public D1? D1
        {
            get; set;
        }

        public D2? D2
        {
            get; set;
        }
    }

    internal class AllowingCircularDependencies
    {
        public D1? D1
        {
            get; set;
        }

        public D2? D2
        {
            get; set;
        }
    }

    internal class UnregisteredProperties
    {
        public D1? D1
        {
            get; set;
        }

        public IDisposable? Unregistered
        {
            get; set;
        }

        public string? AlsoUnregistered
        {
            get; set;
        }
    }
}
