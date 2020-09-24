using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Benchmarks
{
    /// <summary>
    /// Tests the performance of concurrent resolution of a (reasonably) deeply-nested object graph.
    /// </summary>
    public class ConcurrencyBenchmark
    {
        private readonly IContainer _container;

        public ConcurrencyBenchmark()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().SingleInstance();
            builder.RegisterType<B1>();
            builder.RegisterType<B2>();
            builder.RegisterType<C1>();
            builder.RegisterType<C2>();
            builder.RegisterType<D1>();
            builder.RegisterType<D2>();
            _container = builder.Build();
        }

        [Params(100 /*, 100, 1_000 */)]
        public int ResolveTaskCount { get; set; }

        [Params(100 /*, 1_000, 10_000 */)]
        public int ResolvesPerTask { get; set; }

        [Benchmark]
        public async Task MultipleResolvesOnMultipleTasks()
        {
            var tasks = new List<Task>(ResolveTaskCount);

            for (var i = 0; i < ResolveTaskCount; i++)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        for (var j = 0; j < ResolvesPerTask; j++)
                        {
                            var instance = _container.Resolve<A>();
                            Assert.NotNull(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        Assert.True(false, ex.ToString());
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
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
