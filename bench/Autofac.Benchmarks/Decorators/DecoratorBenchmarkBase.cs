using Autofac.Benchmarks.Decorators.Scenario;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Benchmarks.Decorators
{
    public abstract class DecoratorBenchmarkBase<TCommandHandler>
    {
        protected IContainer Container { get; set; }

        [Benchmark]
        public virtual void EnumerableResolve()
        {
            var item = this.Container.Resolve<IEnumerable<TCommandHandler>>();
            GC.KeepAlive(item);
        }

        [Benchmark]
        public virtual void SingleResolve()
        {
            var item = this.Container.Resolve<TCommandHandler>();
            GC.KeepAlive(item);
        }
    }
}
