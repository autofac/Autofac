// This software is part of the Autofac IoC container
// Copyright (c) 2011 Autofac Contributors
// http://autofac.org
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

using BenchmarkDotNet.Running;
using Xunit;

namespace Autofac.Benchmarks
{
    public class Harness
    {
        [Fact]
        public void RootContainerResolve()
        {
            BenchmarkRunner.Run<RootContainerResolveBenchmark>();
        }

        [Fact]
        public void DeepGraphResolve()
        {
            BenchmarkRunner.Run<DeepGraphResolveBenchmark>();
        }

        [Fact]
        public void Decorator_Keyed_Generic()
        {
            BenchmarkRunner.Run<Decorators.KeyedGenericBenchmark>();
        }

        [Fact]
        public void Decorator_Keyed_Nested()
        {
            BenchmarkRunner.Run<Decorators.KeyedNestedBenchmark>();
        }

        [Fact]
        public void Decorator_Keyed_Simple()
        {
            BenchmarkRunner.Run<Decorators.KeyedSimpleBenchmark>();
        }

        [Fact]
        public void Decorator_Keyless_Generic()
        {
            BenchmarkRunner.Run<Decorators.KeylessGenericBenchmark>();
        }

        [Fact]
        public void Decorator_Keyless_Nested()
        {
            BenchmarkRunner.Run<Decorators.KeylessNestedBenchmark>();
        }

        [Fact]
        public void Decorator_Keyless_Nested_Lambda()
        {
            BenchmarkRunner.Run<Decorators.KeylessNestedLambdaBenchmark>();
        }

        [Fact]
        public void Decorator_Keyless_Simple()
        {
            BenchmarkRunner.Run<Decorators.KeylessSimpleBenchmark>();
        }

        [Fact]
        public void Decorator_Keyless_Simple_Lambda()
        {
            BenchmarkRunner.Run<Decorators.KeylessSimpleLambdaBenchmark>();
        }
    }
}