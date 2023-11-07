// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Autofac.Core;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;

namespace Autofac.Test.Diagnostics;

public class DiagnosticSourceExtensionsTests
{
    [Fact]
    public void MiddlewareFailure_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.MiddlewareFailure);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var context = MockResolveRequestContext();
        source.MiddlewareFailure(context, Substitute.For<IResolveMiddleware>());
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.MiddlewareFailure, e.Key);
        Assert.IsType<MiddlewareDiagnosticData>(e.Value);
    }

    [Fact]
    public void MiddlewareStart_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.MiddlewareStart);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var context = MockResolveRequestContext();
        source.MiddlewareStart(context, Substitute.For<IResolveMiddleware>());
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.MiddlewareStart, e.Key);
        Assert.IsType<MiddlewareDiagnosticData>(e.Value);
    }

    [Fact]
    public void MiddlewareSuccess_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.MiddlewareSuccess);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var context = MockResolveRequestContext();
        source.MiddlewareSuccess(context, Substitute.For<IResolveMiddleware>());
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.MiddlewareSuccess, e.Key);
        Assert.IsType<MiddlewareDiagnosticData>(e.Value);
    }

    [Fact]
    public void OperationFailure_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.OperationFailure);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        source.OperationFailure(operation, new DivideByZeroException());
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.OperationFailure, e.Key);
        Assert.IsType<OperationFailureDiagnosticData>(e.Value);
    }

    [Fact]
    public void OperationStart_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.OperationStart);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        var request = MockResolveRequest();
        source.OperationStart(operation, request);
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.OperationStart, e.Key);
        Assert.IsType<OperationStartDiagnosticData>(e.Value);
    }

    [Fact]
    public void OperationSuccess_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.OperationSuccess);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        source.OperationSuccess(operation, "instance");
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.OperationSuccess, e.Key);
        Assert.IsType<OperationSuccessDiagnosticData>(e.Value);
    }

    [Fact]
    public void RequestFailure_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.RequestFailure);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        var context = MockResolveRequestContext();
        source.RequestFailure(operation, context, new DivideByZeroException());
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.RequestFailure, e.Key);
        Assert.IsType<RequestFailureDiagnosticData>(e.Value);
    }

    [Fact]
    public void RequestStart_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.RequestStart);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        var context = MockResolveRequestContext();
        source.RequestStart(operation, context);
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.RequestStart, e.Key);
        Assert.IsType<RequestDiagnosticData>(e.Value);
    }

    [Fact]
    public void RequestSuccess_CorrectEventContent()
    {
        var source = new DiagnosticListener("Autofac");
        var subscriber = new MockSubscriber();
        subscriber.Enable(DiagnosticEventKeys.RequestSuccess);
        source.Subscribe(subscriber, subscriber.IsEnabled);

        var operation = MockResolveOperation();
        var context = MockResolveRequestContext();
        source.RequestSuccess(operation, context);
        var e = Assert.Single(subscriber.Events);
        Assert.Equal(DiagnosticEventKeys.RequestSuccess, e.Key);
        Assert.IsType<RequestDiagnosticData>(e.Value);
    }

    private static ResolveOperation MockResolveOperation()
    {
        var container = new ContainerBuilder().Build();
        var scope = container.BeginLifetimeScope() as ISharingLifetimeScope;
        return new ResolveOperation(scope, container.DiagnosticSource);
    }

    private static ResolveRequest MockResolveRequest()
    {
        return new ResolveRequest(
            new TypedService(typeof(string)),
            new ServiceRegistration(Substitute.For<IResolvePipeline>(), Substitute.For<IComponentRegistration>()),
            Enumerable.Empty<Parameter>());
    }

    private static DefaultResolveRequestContext MockResolveRequestContext()
    {
        var operation = MockResolveOperation();
        var request = MockResolveRequest();
        return new DefaultResolveRequestContext(operation, request, operation.CurrentScope, operation.DiagnosticSource);
    }

    private class MockSubscriber : DiagnosticTracerBase
    {
        public List<KeyValuePair<string, object>> Events { get; } = new List<KeyValuePair<string, object>>();

        protected override void Write(string diagnosticName, object data)
        {
            Events.Add(new KeyValuePair<string, object>(diagnosticName, data));
        }
    }
}
