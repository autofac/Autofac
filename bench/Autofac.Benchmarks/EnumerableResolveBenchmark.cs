using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks
{
    public class EnumerableResolveBenchmark
    {
        private IContainer _container;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().As<IService>();
            builder.RegisterType<B>().As<IService>();
            builder.RegisterType<C>().As<IService>();
            builder.RegisterType<D>().As<IService>();
            builder.RegisterType<E>().As<IService>();
            builder.RegisterType<F>().As<IService>();
            _container = builder.Build();
        }

        [Benchmark]
        public object ResolveIEnumerable()
        {
            return _container.Resolve<IEnumerable<IService>>();
        }

        [Benchmark]
        public object ResolveIReadOnlyList()
        {
            return _container.Resolve<IReadOnlyList<IService>>();
        }

        [Benchmark]
        public object ResolveArray()
        {
            return _container.Resolve<IService[]>();
        }

        internal interface IService
        {
        }

        internal class A : IService
        {
        }

        internal class B : IService
        {
        }

        internal class C : IService
        {
        }

        internal class D : IService
        {
        }

        internal class E : IService
        {
        }

        internal class F : IService
        {
        }
    }
}
