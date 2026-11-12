// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Benchmarks.Decorators;

namespace Autofac.Benchmarks;

public static class BenchmarkSet
{
    public static readonly Type[] All =
    {
        typeof(ChildScopeResolveBenchmark),
        typeof(KeyedAnyKeyChildScopeBenchmark),
        typeof(ContainerBuildBenchmark),
        typeof(ContainerBuildInheritedMembersBenchmark),
        typeof(AssemblyScanningBenchmark),
        typeof(ConcurrencyBenchmark),
        typeof(ConcurrencyNestedScopeBenchmark),
        typeof(CoreResolvePathBenchmark),
        typeof(KeyedGenericBenchmark),
        typeof(KeyedAnyKeySimpleBenchmark),
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
        typeof(KeylessConditionalBenchmark),
        typeof(KeylessParameterBenchmark),
        typeof(KeylessGenericUnmatchedBenchmark),
        typeof(CompositeBenchmark),
        typeof(DeepGraphResolveBenchmark),
        typeof(EnumerableResolveBenchmark),
        typeof(PropertyInjectionBenchmark),
        typeof(RootContainerResolveBenchmark),
        typeof(ResolvePipelineAllocationBenchmark),
        typeof(OpenGenericBenchmark),
        typeof(OpenGenericMultiServiceBenchmark),
        typeof(OpenGenericBindingBenchmark),
        typeof(MultiConstructorBenchmark),
        typeof(ConstructorParameterBenchmark),
        typeof(LambdaResolveBenchmark),
        typeof(RequiredPropertyBenchmark),
        typeof(ModuleRegistrationBenchmark),
        typeof(GeneratedFactoryChildScopeBenchmark),
        typeof(ImplicitRelationshipBenchmark),
    };
}
