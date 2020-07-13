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
using System.Linq;
using Autofac.Core;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;
using Moq;
using Xunit;

namespace Autofac.Test.Core.Diagnostics
{
    public class DotDiagnosticTracerTests
    {
        [Fact]
        public void OnOperationStart_AddsOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnOperationStart(new OperationStartDiagnosticData(MockResolveOperation(), MockResolveRequest()));
            Assert.Equal(1, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnOperationStart_NoDataSkipsOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnOperationStart(null);
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnOperationSuccess_CompletesOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var called = false;
            tracer.OperationCompleted += (sender, args) =>
            {
                called = true;
            };
            var op = MockResolveOperation();
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnOperationSuccess(new OperationSuccessDiagnosticData(op, "instance"));
            Assert.Equal(0, tracer.OperationsInProgress);
            Assert.True(called);
        }

        [Fact]
        public void OnOperationSuccess_NoDataSkipsOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var op = MockResolveOperation();
            var called = false;
            tracer.OperationCompleted += (sender, args) =>
            {
                called = true;
            };

            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnOperationSuccess(null);
            Assert.Equal(1, tracer.OperationsInProgress);
            Assert.False(called);
        }

        [Fact]
        public void OnOperationFailure_CompletesOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var called = false;
            tracer.OperationCompleted += (sender, args) =>
            {
                called = true;
            };
            var op = MockResolveOperation();
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnOperationFailure(new OperationFailureDiagnosticData(op, new DivideByZeroException()));
            Assert.Equal(0, tracer.OperationsInProgress);
            Assert.True(called);
        }

        [Fact]
        public void OnOperationFailure_NoDataSkipsOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var op = MockResolveOperation();
            var called = false;
            tracer.OperationCompleted += (sender, args) =>
            {
                called = true;
            };

            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnOperationFailure(null);
            Assert.Equal(1, tracer.OperationsInProgress);
            Assert.False(called);
        }

        [Fact]
        public void OnRequestStart_NoOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnRequestStart(new RequestDiagnosticData(MockResolveOperation(), MockResolveRequestContext()));
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnRequestSuccess_NoStartOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var op = MockResolveOperation();
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));

            // Should have a request start before ending, but make sure we don't
            // explode if something weird happens.
            tracer.OnRequestSuccess(new RequestDiagnosticData(op, MockResolveRequestContext()));
        }

        [Fact]
        public void OnRequestFailure_NoStartOperation()
        {
            var tracer = new DotDiagnosticTracer();
            var op = MockResolveOperation();
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));

            // Should have a request start before ending, but make sure we don't
            // explode if something weird happens.
            tracer.OnRequestFailure(new RequestFailureDiagnosticData(op, MockResolveRequestContext(), new DivideByZeroException()));
        }

        [Fact]
        public void OnMiddlewareStart_NoOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnMiddlewareStart(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnMiddlewareStart_NoRequest()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnOperationStart(new OperationStartDiagnosticData(MockResolveOperation(), MockResolveRequest()));

            // Middleware only happens during a request, but in the event we missed the start...
            tracer.OnMiddlewareStart(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
        }

        [Fact]
        public void OnMiddlewareSuccess_NoOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnMiddlewareSuccess(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnMiddlewareSuccess_NoRequest()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnOperationStart(new OperationStartDiagnosticData(MockResolveOperation(), MockResolveRequest()));

            // Middleware only happens during a request, but in the event we missed the start...
            tracer.OnMiddlewareSuccess(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
        }

        [Fact]
        public void OnMiddlewareSuccess_NoMiddleware()
        {
            var tracer = new DotDiagnosticTracer();
            var reqCtx = MockResolveRequestContext();
            var op = reqCtx.Operation;
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnRequestStart(new RequestDiagnosticData(op, reqCtx));

            // Middleware should have a start before success...
            tracer.OnMiddlewareSuccess(new MiddlewareDiagnosticData(reqCtx, Mock.Of<IResolveMiddleware>()));
        }

        [Fact]
        public void OnMiddlewareFailure_NoOperation()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnMiddlewareFailure(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
            Assert.Equal(0, tracer.OperationsInProgress);
        }

        [Fact]
        public void OnMiddlewareFailure_NoRequest()
        {
            var tracer = new DotDiagnosticTracer();
            tracer.OnOperationStart(new OperationStartDiagnosticData(MockResolveOperation(), MockResolveRequest()));

            // Middleware only happens during a request, but in the event we missed the start...
            tracer.OnMiddlewareFailure(new MiddlewareDiagnosticData(MockResolveRequestContext(), Mock.Of<IResolveMiddleware>()));
        }

        [Fact]
        public void OnMiddlewareFailure_NoMiddleware()
        {
            var tracer = new DotDiagnosticTracer();
            var reqCtx = MockResolveRequestContext();
            var op = reqCtx.Operation;
            tracer.OnOperationStart(new OperationStartDiagnosticData(op, MockResolveRequest()));
            tracer.OnRequestStart(new RequestDiagnosticData(op, reqCtx));

            // Middleware should have a start before success...
            tracer.OnMiddlewareFailure(new MiddlewareDiagnosticData(reqCtx, Mock.Of<IResolveMiddleware>()));
        }

        private static ResolveOperation MockResolveOperation()
        {
            var container = new ContainerBuilder().Build();
            var scope = container.BeginLifetimeScope() as ISharingLifetimeScope;
            return new ResolveOperation(scope);
        }

        private static ResolveRequest MockResolveRequest()
        {
            return new ResolveRequest(
                new TypedService(typeof(string)),
                new ServiceRegistration(Mock.Of<IResolvePipeline>(), Mock.Of<IComponentRegistration>()),
                Enumerable.Empty<Parameter>());
        }

        private static ResolveRequestContext MockResolveRequestContext()
        {
            var operation = MockResolveOperation();
            var request = MockResolveRequest();
            return new ResolveRequestContext(operation, request, operation.CurrentScope);
        }
    }
}
