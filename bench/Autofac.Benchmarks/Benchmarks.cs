using System;
using Autofac.Benchmarks.Decorators;

namespace Autofac.Benchmarks
{
    internal static class Benchmarks
    {
        internal static readonly Type[] All =
        {
            typeof(ChildScopeResolveBenchmark),
            typeof(ConcurrencyBenchmark),
            typeof(KeyedGenericBenchmark),
            typeof(KeyedNestedBenchmark),
            typeof(KeyedSimpleBenchmark),
            typeof(KeylessGenericBenchmark),
            typeof(KeylessNestedBenchmark),
            typeof(KeylessNestedLambdaBenchmark),
            typeof(KeylessSimpleBenchmark),
            typeof(KeylessSimpleLambdaBenchmark),
            typeof(DeepGraphResolveBenchmark),
            typeof(EnumerableResolveBenchmark),
            typeof(PropertyInjectionBenchmark),
            typeof(RootContainerResolveBenchmark),
            typeof(OpenGenericBenchmark)
        };
    }
}