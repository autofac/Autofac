// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Diagnostics;
using Xunit;

namespace Autofac.Specification.Test.Diagnostics
{
    public class DefaultDiagnosticTracerTests
    {
        [Fact]
        public void DiagnosticTracerRaisesEvents()
        {
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(ctxt => "Hello");

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);

            string lastOpResult = null;

            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
                Assert.True(args.OperationSucceeded);
            };

            container.Resolve<string>();

            Assert.Contains("Hello", lastOpResult);
        }

        [Fact]
        public void DiagnosticTracerRaisesEventsOnError()
        {
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<string>(ctxt => throw new InvalidOperationException());

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);

            string lastOpResult = null;

            tracer.OperationCompleted += (sender, args) =>
            {
                Assert.Same(tracer, sender);
                lastOpResult = args.TraceContent;
                Assert.False(args.OperationSucceeded);
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
        public void DiagnosticTracerHandlesDecorators()
        {
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

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
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

            container.SubscribeToDiagnostics(tracer);
            container.Resolve<IService>();

            // The dictionary of tracked operations and
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
