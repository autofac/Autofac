using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Benchmarks
{
    public class MultiConstructorBenchmark
    {
        private IContainer _container;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<OneConstructor>();
            builder.RegisterType<TwoConstructors>();
            builder.RegisterType<ThreeConstructors>();
            builder.RegisterType<TwoValidConstructorsOneInvalid>();
            builder.RegisterType<FourConstructors>();
            builder.RegisterType<D1>();
            builder.RegisterType<D2>();
            builder.RegisterType<D3>();
            builder.RegisterType<D4>();

            _container = builder.Build();
        }

        [Benchmark(Baseline = true)]
        public void Single()
        {
            _container.Resolve<OneConstructor>();
        }

        [Benchmark]
        public void Two()
        {
            _container.Resolve<TwoConstructors>();
        }

        [Benchmark]
        public void TwoValidOneInvalid()
        {
            _container.Resolve<TwoValidConstructorsOneInvalid>();
        }

        [Benchmark]
        public void Three()
        {
            _container.Resolve<ThreeConstructors>();
        }

        [Benchmark]
        public void Four()
        {
            _container.Resolve<FourConstructors>();
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        private class OneConstructor
        {
            public OneConstructor(D1 d1)
            {
            }
        }

        private class TwoConstructors
        {
            public TwoConstructors(D1 d1)
            {
            }

            public TwoConstructors(D1 d1, D2 d2)
            {
            }
        }

        private class TwoValidConstructorsOneInvalid
        {
            public TwoValidConstructorsOneInvalid(D1 d1)
            {
            }

            public TwoValidConstructorsOneInvalid(D1 d1, D2 d2)
            {
            }

            public TwoValidConstructorsOneInvalid(D1 d1, D2 d2, object notRegistered)
            {
            }
        }

        private class ThreeConstructors
        {
            public ThreeConstructors(D1 d1)
            {
            }

            public ThreeConstructors(D1 d1, D2 d2)
            {
            }

            public ThreeConstructors(D1 d1, D2 d2, D3 d3)
            {
            }
        }

        private class FourConstructors
        {
            public FourConstructors(D1 d1)
            {
            }

            public FourConstructors(D1 d1, D2 d2)
            {
            }

            public FourConstructors(D1 d1, D2 d2, D3 d3)
            {
            }

            public FourConstructors(D1 d1, D2 d2, D3 d3, D4 d4)
            {
            }
        }

        private class D1
        {
        }

        private class D2
        {
        }

        private class D3
        {
        }

        private class D4
        {
        }

#pragma warning restore IDE0060
    }
}
