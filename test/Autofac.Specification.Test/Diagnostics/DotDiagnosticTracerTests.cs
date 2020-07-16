// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Diagnostics;
using Xunit;

namespace Autofac.Specification.Test.Diagnostics
{
    public class DotDiagnosticTracerTests
    {
        [Fact]
        public void DiagnosticTracerRaisesEventsOnSuccess()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(ctxt => "Hello");
            var container = containerBuilder.Build();

            var tracer = new DotDiagnosticTracer();
            container.SubscribeToDiagnostics(tracer);
            string lastOpResult = null;
            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
            };

            container.Resolve<string>();

            Assert.Contains("λ:System.String", lastOpResult);
            Assert.StartsWith("digraph G {", lastOpResult);
            Assert.EndsWith("}", lastOpResult.Trim());
        }

        [Fact]
        public void DiagnosticTracerRaisesEventsOnError()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<string>(ctxt => throw new InvalidOperationException());
            var container = containerBuilder.Build();

            var tracer = new DotDiagnosticTracer();
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
            Assert.StartsWith("digraph G {", lastOpResult);
            Assert.EndsWith("}", lastOpResult.Trim());
        }

        [Fact]
        public void DiagnosticTracerHandlesDecorators()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();
            var container = containerBuilder.Build();

            var tracer = new DotDiagnosticTracer();
            container.SubscribeToDiagnostics(tracer);
            string lastOpResult = null;
            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
            };

            container.Resolve<IService>();

            Assert.Contains("Decorator", lastOpResult);
        }

        [Fact]
        public void DiagnosticTracerDoesNotLeakMemory()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();
            var container = containerBuilder.Build();

            var tracer = new DotDiagnosticTracer();
            container.SubscribeToDiagnostics(tracer);
            container.Resolve<IService>();

            // The dictionary of tracked operations and
            // graphs should be empty.
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
