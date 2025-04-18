﻿using Autofac.Benchmarks.Decorators;

namespace Autofac.Benchmarks;

public static class BenchmarkSet
{
    public static readonly Type[] All =
    {
        typeof(ChildScopeResolveBenchmark),
        typeof(ConcurrencyBenchmark),
        typeof(ConcurrencyNestedScopeBenchmark),
        typeof(KeyedGenericBenchmark),
        typeof(KeyedNestedBenchmark),
        typeof(KeyedSimpleBenchmark),
        typeof(KeylessGenericBenchmark),
        typeof(KeylessNestedBenchmark),
        typeof(KeylessNestedSharedInstanceBenchmark),
        typeof(KeylessNestedLambdaBenchmark),
        typeof(KeylessNestedSharedInstanceLambdaBenchmark),
        typeof(KeylessSimpleBenchmark),
        typeof(KeylessSimpleSharedInstanceBenchmark),
        typeof(KeylessSimpleLambdaBenchmark),
        typeof(KeylessSimpleSharedInstanceLambdaBenchmark),
        typeof(DeepGraphResolveBenchmark),
        typeof(EnumerableResolveBenchmark),
        typeof(PropertyInjectionBenchmark),
        typeof(RootContainerResolveBenchmark),
        typeof(OpenGenericBenchmark),
        typeof(MultiConstructorBenchmark),
        typeof(LambdaResolveBenchmark),
        typeof(RequiredPropertyBenchmark),
    };
}
