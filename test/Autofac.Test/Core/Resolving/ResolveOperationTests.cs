// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Autofac.Core;
using Autofac.Core.Resolving;

namespace Autofac.Test.Core.Resolving;

public class ResolveOperationTests
{
    [Fact]
    public void NullLifetimeScope_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ResolveOperation(null!, new DiagnosticListener("SomeListener")));
        Assert.Contains("(Parameter 'mostNestedLifetimeScope')", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NullDiagnosticSource_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ResolveOperation(Substitute.For<ISharingLifetimeScope>(), null!));
        Assert.Contains("(Parameter 'diagnosticSource')", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyInProgressRequestWhenInitializing()
    {
        var resolveOperation = new ResolveOperation(Substitute.For<ISharingLifetimeScope>(), new DiagnosticListener("SomeName"));

        var inProgressStack = resolveOperation.InProgressRequests;

        Assert.Empty(inProgressStack);
    }

    [Fact]
    public void AfterTheOperationIsFinished_ReusingTheTemporaryContextThrows()
    {
        IComponentContext ctx = null;
        var builder = new ContainerBuilder();
        builder.Register(c =>
        {
            ctx = c;
            return new object();
        });
        builder.RegisterInstance("Hello");
        var container = builder.Build();
        container.Resolve<string>();
        container.Resolve<object>();
        Assert.Throws<ObjectDisposedException>(() => ctx.Resolve<string>());
    }

    [Fact]
    public void OperationRaisesSuccessTraceEvents()
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance("Hello");

        var container = builder.Build();
        var mockTracer = Mocks.GetTracer();
        container.SubscribeToDiagnostics(mockTracer);

        var scope = container.Resolve<ILifetimeScope>() as ISharingLifetimeScope;

        var resolveOp = new ResolveOperation(scope, container.DiagnosticSource);

        var raisedEvents = new List<string>();

        var request = new ResolveRequest(new TypedService(typeof(string)), scope.ResolvableImplementationFor<string>(), Enumerable.Empty<Parameter>());
        var request2 = new ResolveRequest(new TypedService(typeof(int)), scope.ResolvableImplementationFor<string>(), Enumerable.Empty<Parameter>());

        mockTracer.OperationStarting += (op, req) =>
        {
            raisedEvents.Add("op-start");
            Assert.Equal(resolveOp, op);
            Assert.Equal(request, req);
            Assert.True(req != request2);
        };

        mockTracer.RequestStarting += (op, context) =>
        {
            raisedEvents.Add("req-start");
            Assert.Equal(resolveOp, op);
            Assert.Equal(request.Service, context.Service);
        };

        mockTracer.RequestSucceeding += (op, context) =>
        {
            raisedEvents.Add("req-success");
            Assert.Equal(resolveOp, op);
        };

        mockTracer.OperationSucceeding += (op, instance) =>
        {
            raisedEvents.Add("op-success");
            Assert.Equal("Hello", instance);
        };

        resolveOp.Execute(request);

        Assert.Equal(new[] { "op-start", "req-start", "req-success", "op-success" }, raisedEvents);
    }

    [Fact]
    public void OperationRaisesFailureTraceEvents()
    {
        var builder = new ContainerBuilder();

        builder.Register<string>(context => throw new InvalidOperationException());

        var container = builder.Build();
        var mockTracer = Mocks.GetTracer();
        container.SubscribeToDiagnostics(mockTracer);

        var scope = container.Resolve<ILifetimeScope>() as ISharingLifetimeScope;

        var resolveOp = new ResolveOperation(scope, container.DiagnosticSource);

        var raisedEvents = new List<string>();

        var request = new ResolveRequest(new TypedService(typeof(string)), scope.ResolvableImplementationFor<string>(), Enumerable.Empty<Parameter>());

        mockTracer.OperationStarting += (op, req) =>
        {
            raisedEvents.Add("op-start");
            Assert.Equal(resolveOp, op);
            Assert.Equal(request, req);
        };

        mockTracer.RequestStarting += (op, context) =>
        {
            raisedEvents.Add("req-start");
            Assert.Equal(resolveOp, op);
            Assert.Equal(request.Service, context.Service);
        };

        mockTracer.RequestFailing += (op, context, ex) =>
        {
            raisedEvents.Add("req-fail");
            Assert.Equal(resolveOp, op);
            Assert.IsType<DependencyResolutionException>(ex);
        };

        mockTracer.OperationFailing += (op, ex) =>
        {
            raisedEvents.Add("op-fail");
            Assert.IsType<DependencyResolutionException>(ex);
        };

        try
        {
            resolveOp.Execute(request);
        }
        catch
        {
        }

        Assert.Equal(new[] { "op-start", "req-start", "req-fail", "op-fail" }, raisedEvents);
    }
}
