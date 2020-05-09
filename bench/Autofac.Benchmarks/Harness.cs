// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A1 PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Linq;
using Autofac.Benchmarks.Decorators;
using BenchmarkDotNet.Running;
using Xunit;

namespace Autofac.Benchmarks
{
    public class Harness
    {
        [Fact]
        public void ChildScopeResolve() => RunBenchmark<ChildScopeResolveBenchmark>();

        [Fact]
        public void Concurrency() => RunBenchmark<ConcurrencyBenchmark>();

        [Fact]
        public void ConcurrencyNestedScopes() => RunBenchmark<ConcurrencyNestedScopeBenchmark>();

        [Fact]
        public void Decorator_Keyed_Generic() => RunBenchmark<KeyedGenericBenchmark>();

        [Fact]
        public void Decorator_Keyed_Nested() => RunBenchmark<KeyedNestedBenchmark>();

        [Fact]
        public void Decorator_Keyed_Simple() => RunBenchmark<KeyedSimpleBenchmark>();

        [Fact]
        public void Decorator_Keyless_Generic() => RunBenchmark<KeylessGenericBenchmark>();

        [Fact]
        public void Decorator_Keyless_Nested() => RunBenchmark<KeylessNestedBenchmark>();

        [Fact]
        public void Decorator_Keyless_Nested_Shared_Instance() => RunBenchmark<KeylessNestedSharedInstanceBenchmark>();

        [Fact]
        public void Decorator_Keyless_Nested_Lambda() => RunBenchmark<KeylessNestedLambdaBenchmark>();

        [Fact]
        public void Decorator_Keyless_Nested_Lambda_Shared_Instance() => RunBenchmark<KeylessNestedSharedInstanceLambdaBenchmark>();

        [Fact]
        public void Decorator_Keyless_Simple() => RunBenchmark<KeylessSimpleBenchmark>();

        [Fact]
        public void Decorator_Keyless_Simple_Shared_Instance() => RunBenchmark<KeylessSimpleSharedInstanceBenchmark>();

        [Fact]
        public void Decorator_Keyless_Simple_Lambda() => RunBenchmark<KeylessSimpleLambdaBenchmark>();

        [Fact]
        public void Decorator_Keyless_Simple_Lambda_Shared_Instance() => RunBenchmark<KeylessSimpleSharedInstanceLambdaBenchmark>();

        [Fact]
        public void DeepGraphResolve() => RunBenchmark<DeepGraphResolveBenchmark>();

        [Fact]
        public void EnumerableResolve() => RunBenchmark<EnumerableResolveBenchmark>();

        [Fact]
        public void PropertyInjection() => RunBenchmark<PropertyInjectionBenchmark>();

        [Fact]
        public void RootContainerResolve() => RunBenchmark<RootContainerResolveBenchmark>();

        [Fact]
        public void OpenGeneric() => RunBenchmark<OpenGenericBenchmark>();

        /// <remarks>
        /// This method is used to enforce that benchmark types are added to <see cref="Benchmarks.All"/>
        /// so that they can be used directly from the command line in <see cref="Program.Main"/> as well.
        /// </remarks>
        private static void RunBenchmark<TBenchmark>()
        {
            var targetType = typeof(TBenchmark);
            var benchmarkType = Benchmarks.All.Single(type => type == targetType);
            BenchmarkRunner.Run(benchmarkType, new BenchmarkConfig());
        }
    }
}
