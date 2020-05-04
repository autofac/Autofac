using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Autofac.Benchmarks.Decorators
{
    public abstract class DecoratorBenchmarkBase<TCommandHandler>
    {
        protected IContainer Container { get; set; }

        [Benchmark]
        [Arguments(1)]
        [Arguments(2)]
        [Arguments(3)]
        public virtual void ResolveEnumerableT(int repetitions)
        {
            var iteration = 1;
            object item = null;
            while(iteration++ < repetitions)
            {
                item = this.Container.Resolve<IEnumerable<TCommandHandler>>();
                GC.KeepAlive(item);
            }
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(2)]
        [Arguments(3)]
        public virtual void ResolveT(int repetitions)
        {
            var iteration = 1;
            object item = null;
            while (iteration++ < repetitions)
            {
                item = this.Container.Resolve<TCommandHandler>();
                GC.KeepAlive(item);
            }
        }
    }
}
