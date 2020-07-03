using System;
using Autofac.Core.Diagnostics;
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

            container.AttachTrace(tracer);

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
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<string>(ctxt => throw new InvalidOperationException());

            var container = containerBuilder.Build();

            container.AttachTrace(tracer);

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
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

            container.AttachTrace(tracer);

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
            var tracer = new DefaultDiagnosticTracer();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Implementor>().As<IService>();
            containerBuilder.RegisterDecorator<Decorator, IService>();

            var container = containerBuilder.Build();

            container.AttachTrace(tracer);
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
                this.Decorated = decorated;
            }

            public IService Decorated { get; }
        }

        private class Implementor : IService
        {
        }
    }
}
