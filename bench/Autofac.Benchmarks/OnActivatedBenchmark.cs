using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Autofac.Benchmarks
{
    public class OnActivatedBenchmark
    {
        [Benchmark]
        public void ResolveWithOnActivatedWithAction()
        {
            var builder = new ContainerBuilder();
            Action<ContainerBuilder> someAction;
            builder
                .RegisterType<FakeService>()
                .OnActivated(c =>
                {
                    someAction = b =>
                    {
                        b.RegisterInstance(c.Instance);
                    };
                });
            using var container = builder.Build();
            container.Resolve<FakeService>();
        }

        internal class FakeService
        {
            private IEnumerable<int> _data;
            public FakeService()
            {
                _data = new int[1000];
            }
        }
    }
}
