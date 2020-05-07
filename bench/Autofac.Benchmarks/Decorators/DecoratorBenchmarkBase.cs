using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Autofac.Benchmarks.Decorators
{
    public abstract class DecoratorBenchmarkBase<TCommandHandler>
    {
        protected IContainer Container { get; set; }

        [Benchmark(Baseline =true)]
        public virtual void Baseline()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                //NO-OP for baseline measurement
            }
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(2)]
        [Arguments(3)]
        public virtual void ResolveEnumerableT(int repetitions)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var iteration = 0;
                object item = null;
                while (iteration++ < repetitions)
                {
                    item = scope.Resolve<IEnumerable<TCommandHandler>>();
                    GC.KeepAlive(item);
                }
            }
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(2)]
        [Arguments(3)]
        public virtual void ResolveT(int repetitions)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var iteration = 0;
                object item = null;
                while (iteration++ < repetitions)
                {
                    item = scope.Resolve<TCommandHandler>();
                    GC.KeepAlive(item);
                }
            }
        }
    }
}
