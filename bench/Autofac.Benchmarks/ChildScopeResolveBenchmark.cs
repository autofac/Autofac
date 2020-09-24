using System;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks
{
    public class ChildScopeResolveBenchmark
    {
        private IContainer _container;

        [Benchmark]
        public void Resolve()
        {
            using (var requestScope = _container.BeginLifetimeScope("request", b => b.RegisterType<C1>()))
            {
                using (var unitOfWorkScope = requestScope.BeginLifetimeScope())
                {
                    var instance = unitOfWorkScope.Resolve<A>();
                    GC.KeepAlive(instance);
                }
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();
            builder.RegisterType<B1>();
            builder.RegisterType<B2>().InstancePerMatchingLifetimeScope("request");
            builder.RegisterType<C2>().InstancePerLifetimeScope();
            builder.RegisterType<D1>().SingleInstance();
            builder.RegisterType<D2>().SingleInstance();
            _container = builder.Build();
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

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

#pragma warning restore IDE0060

    }
}
