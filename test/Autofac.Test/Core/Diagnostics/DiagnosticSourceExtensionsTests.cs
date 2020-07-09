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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;
using Moq;
using Xunit;

namespace Autofac.Test.Core.Diagnostics
{
    public class DiagnosticSourceExtensionsTests
    {
        [InlineData(DiagnosticEventKeys.MiddlewareFailure)]
        [InlineData(DiagnosticEventKeys.MiddlewareStart)]
        [InlineData(DiagnosticEventKeys.MiddlewareSuccess)]
        [Theory]
        public void MiddlewareDiagnosticsEnabled_IsEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.True(source.MiddlewareDiagnosticsEnabled());
        }

        [InlineData(DiagnosticEventKeys.OperationFailure)]
        [InlineData(DiagnosticEventKeys.OperationStart)]
        [InlineData(DiagnosticEventKeys.OperationSuccess)]
        [InlineData(DiagnosticEventKeys.RequestFailure)]
        [InlineData(DiagnosticEventKeys.RequestStart)]
        [InlineData(DiagnosticEventKeys.RequestSuccess)]
        [Theory]
        public void MiddlewareDiagnosticsEnabled_IsNotEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.False(source.MiddlewareDiagnosticsEnabled());
        }

        [InlineData(DiagnosticEventKeys.OperationFailure)]
        [InlineData(DiagnosticEventKeys.OperationStart)]
        [InlineData(DiagnosticEventKeys.OperationSuccess)]
        [Theory]
        public void OperationDiagnosticsEnabled_IsEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.True(source.OperationDiagnosticsEnabled());
        }

        [InlineData(DiagnosticEventKeys.MiddlewareFailure)]
        [InlineData(DiagnosticEventKeys.MiddlewareStart)]
        [InlineData(DiagnosticEventKeys.MiddlewareSuccess)]
        [InlineData(DiagnosticEventKeys.RequestFailure)]
        [InlineData(DiagnosticEventKeys.RequestStart)]
        [InlineData(DiagnosticEventKeys.RequestSuccess)]
        [Theory]
        public void OperationDiagnosticsEnabled_IsNotEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.False(source.OperationDiagnosticsEnabled());
        }

        [InlineData(DiagnosticEventKeys.RequestFailure)]
        [InlineData(DiagnosticEventKeys.RequestStart)]
        [InlineData(DiagnosticEventKeys.RequestSuccess)]
        [Theory]
        public void RequestDiagnosticsEnabled_IsEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.True(source.RequestDiagnosticsEnabled());
        }

        [InlineData(DiagnosticEventKeys.MiddlewareFailure)]
        [InlineData(DiagnosticEventKeys.MiddlewareStart)]
        [InlineData(DiagnosticEventKeys.MiddlewareSuccess)]
        [InlineData(DiagnosticEventKeys.OperationFailure)]
        [InlineData(DiagnosticEventKeys.OperationStart)]
        [InlineData(DiagnosticEventKeys.OperationSuccess)]
        [Theory]
        public void RequestDiagnosticsEnabled_IsNotEnabled(string key)
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(key);
            source.Subscribe(subscriber, subscriber.IsEnabled);
            Assert.False(source.RequestDiagnosticsEnabled());
        }

        [Fact]
        public void MiddlewareFailure_CorrectEventContent()
        {
            var source = new DiagnosticListener("Autofac");
            var subscriber = new MockSubscriber();
            subscriber.Enable(DiagnosticEventKeys.MiddlewareFailure);
            source.Subscribe(subscriber, subscriber.IsEnabled);

            var context = MockResolveRequestContext();
            source.MiddlewareFailure(context, Mock.Of<IResolveMiddleware>());
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
            source.MiddlewareStart(context, Mock.Of<IResolveMiddleware>());
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
            source.MiddlewareSuccess(context, Mock.Of<IResolveMiddleware>());
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

        private class MockSubscriber : DiagnosticTracerBase
        {
            public List<KeyValuePair<string, object>> Events { get; } = new List<KeyValuePair<string, object>>();

            public override void Write(string diagnosticName, object data)
            {
                Events.Add(new KeyValuePair<string, object>(diagnosticName, data));
            }
        }
    }
}
