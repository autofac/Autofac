using BenchmarkDotNet.Attributes;
using System;

namespace Autofac.Benchmarks
{
    /// <summary>
    /// Tests the performance of retrieving a (reasonably) deeply-nested object graph.
    /// </summary>
    public class DeepGraphResolveBenchmark
    {
        private IContainer _container;

        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();
            builder.RegisterType<B1>();
            builder.RegisterType<B2>();
            builder.RegisterType<C1>();
            builder.RegisterType<C2>();
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
    }

#pragma warning disable SA1402, SA1502

    internal class A
    {
        public A(B1 b1, B2 b2) { }
    }

    internal class B1
    {
        public B1(B2 b2, C1 c1, C2 c2) { }
    }

    internal class B2
    {
        public B2(C1 c1, C2 c2) { }
    }

    internal class C1
    {
        public C1(C2 c2, D1 d1, D2 d2) { }
    }

    internal class C2
    {
        public C2(D1 d1, D2 d2) { }
    }

    internal class D1 { }

    internal class D2 { }

#pragma warning restore SA1402, SA1502
}
