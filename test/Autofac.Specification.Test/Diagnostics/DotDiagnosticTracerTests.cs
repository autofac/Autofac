// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
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

using System;
using Autofac.Core.Diagnostics;
using Xunit;

namespace Autofac.Specification.Test.Diagnostics
{
    public class DotDiagnosticTracerTests
    {
        [Fact]
        public void DiagnosticTracerRaisesEvents()
        {
            var tracer = new DotDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(ctxt => "Hello");

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);

            string lastOpResult = null;

            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
            };

            container.Resolve<string>();

            Assert.Contains("Hello", lastOpResult);
        }

        [Fact]
        public void DiagnosticTracerRaisesEventsOnError()
        {
            var tracer = new DotDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<string>(ctxt => throw new InvalidOperationException());

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);

            string lastOpResult = null;

            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
            };

            try
            {
                container.Resolve<string>();
            }
            catch
            {
            }

            Assert.Contains(nameof(InvalidOperationException), lastOpResult);
        }

        [Fact]
        public void DiagnosticTracerDoesNotRaiseAnEventOnNestedOperations()
        {
            var tracer = new DotDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);

            int traceCount = 0;
            string lastOpResult = null;

            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
                traceCount++;
            };

            container.Resolve<IService>();

            // Only a single trace (despite the nested operations).
            Assert.Equal(1, traceCount);
            Assert.Contains("Decorator", lastOpResult);
        }

        [Fact]
        public void DiagnosticTracerDoesNotLeakMemory()
        {
            var tracer = new DotDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);
            container.Resolve<IService>();

            // The dictionary of tracked trace IDs and
            // string builders should be empty.
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        private interface IService
        {
        }

        private class Decorator : IService
        {
            public Decorator(IService decorated)
            {
                Decorated = decorated;
            }

            public IService Decorated { get; }
        }

        private class Implementor : IService
        {
        }
    }
}
