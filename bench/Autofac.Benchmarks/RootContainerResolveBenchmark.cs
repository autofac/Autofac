using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Autofac.Benchmarks
{
    /// <summary>
    /// Tests the performance of retrieving various simple components from a root container.
    /// </summary>
    public class RootContainerResolveBenchmark
    {
        private IContainer _container;

        [Setup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Shared>().SingleInstance();
            builder.RegisterType<NonSharedReflection>();
            builder.Register(c => new NonSharedDelegate());
            _container = builder.Build();
        }

        [Benchmark(Baseline = true)]
        public void OperatorNew()
        {
            var instance = new NonSharedDelegate();
            GC.KeepAlive(instance);
        }

        [Benchmark]
        public void NonSharedReflection()
        {
            var nonShared = _container.Resolve<NonSharedReflection>();
            GC.KeepAlive(nonShared);
        }

        [Benchmark]
        public void NonSharedDelegate()
        {
            var nonShared = _container.Resolve<NonSharedDelegate>();
            GC.KeepAlive(nonShared);
        }

        [Benchmark]
        public void Shared()
        {
            var shared = _container.Resolve<Shared>();
            GC.KeepAlive(shared);
        }
    }

#pragma warning disable SA1402, SA1502

    internal class NonSharedDelegate { }

    internal class NonSharedReflection { }

    internal class Shared { }

#pragma warning restore SA1402, SA1502
}
