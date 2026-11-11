// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Diagnostics;

namespace Autofac.Specification.Test.Diagnostics;

public class DefaultDiagnosticTracerTests
{
    [Fact]
    public void DiagnosticTracerRaisesEvents()
    {
        var tracer = new DefaultDiagnosticTracer();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(context => "Hello");

        var container = containerBuilder.Build();

        container.SubscribeToDiagnostics(tracer);

        string? lastOpResult = null;

        tracer.OperationCompleted += (sender, args) =>
        {
            Assert.Same(tracer, sender);
            lastOpResult = args.TraceContent;
            Assert.True(args.OperationSucceeded);
        };

        container.Resolve<string>();

        Assert.Contains("Hello", lastOpResult, StringComparison.Ordinal);
    }

    [Fact]
    public void DiagnosticTracerRaisesEventsOnError()
    {
        var tracer = new DefaultDiagnosticTracer();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<string>(context => throw new InvalidOperationException());

        var container = containerBuilder.Build();

        container.SubscribeToDiagnostics(tracer);

        string? lastOpResult = null;

        tracer.OperationCompleted += (sender, args) =>
        {
            Assert.Same(tracer, sender);
            lastOpResult = args.TraceContent;
            Assert.False(args.OperationSucceeded);
        };

        Assert.ThrowsAny<Exception>(() => container.Resolve<string>());
        Assert.Contains(nameof(InvalidOperationException), lastOpResult, StringComparison.Ordinal);
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

        string? lastOpResult = null;

        tracer.OperationCompleted += (sender, args) =>
        {
            Assert.Same(tracer, sender);
            lastOpResult = args.TraceContent;
        };

        container.Resolve<IService>();

        Assert.Contains("Decorator", lastOpResult, StringComparison.Ordinal);
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

    [Fact]
    public void DiagnosticTracerRaisesEventsForCachedSharedInstances()
    {
        var tracer = new DefaultDiagnosticTracer();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<Implementor>().SingleInstance();
        containerBuilder.RegisterType<Consumer>();

        var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();

        container.SubscribeToDiagnostics(tracer);

        var completed = new List<string>();
        tracer.OperationCompleted += (sender, args) => completed.Add(args.TraceContent);

        container.Resolve<Implementor>();
        container.Resolve<Implementor>();
        scope.Resolve<Implementor>();

        // A cached dependency still shows up as a request within the operation.
        container.Resolve<Consumer>();

        Assert.Equal(4, completed.Count);
        Assert.Equal(2, CountOccurrences(completed[3], "Resolve Request Starting"));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = text.IndexOf(value, StringComparison.Ordinal); index >= 0; index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
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

        public IService Decorated
        {
            get;
        }
    }

    private class Implementor : IService
    {
    }

    private class Consumer
    {
        public Consumer(Implementor implementor)
        {
            Implementor = implementor;
        }

        public Implementor Implementor
        {
            get;
        }
    }
}
