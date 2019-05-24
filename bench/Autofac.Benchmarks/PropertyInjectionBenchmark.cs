using System;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks
{
    public class PropertyInjectionBenchmark
    {
        private IContainer _container;

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
            _container = builder.Build();
        }

        [Benchmark]
        public void Resolve()
        {
            var instance = _container.Resolve<A>();
            GC.KeepAlive(instance);
        }

        internal class A
        {
            public B1 B1 { get; set; }

            public B2 B2 { get; set; }
        }

        internal class B1
        {
            public C1 C1 { get; set; }
        }

        internal class B2
        {
            public C2 C2 { get; set; }
        }

        internal class C1
        {
            public D1 D1 { get; set; }
        }

        internal class C2
        {
            public D2 D2 { get; set; }
        }

        internal class D1 { }

        internal class D2 { }
    }
}
