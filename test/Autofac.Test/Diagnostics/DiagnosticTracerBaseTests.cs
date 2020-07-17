// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Diagnostics;
using Xunit;

namespace Autofac.Test.Diagnostics
{
    public class DiagnosticTracerBaseTests
    {
        [InlineData(DiagnosticEventKeys.MiddlewareStart)]
        [InlineData(DiagnosticEventKeys.MiddlewareFailure)]
        [InlineData(DiagnosticEventKeys.MiddlewareSuccess)]
        [InlineData(DiagnosticEventKeys.OperationFailure)]
        [InlineData(DiagnosticEventKeys.OperationStart)]
        [InlineData(DiagnosticEventKeys.OperationSuccess)]
        [InlineData(DiagnosticEventKeys.RequestFailure)]
        [InlineData(DiagnosticEventKeys.RequestStart)]
        [InlineData(DiagnosticEventKeys.RequestSuccess)]
        [Theory]
        public void EnableAll_SubscribesToAllEvents(string key)
        {
            var tracer = new TestTracer();
            tracer.EnableAll();
            Assert.True(tracer.IsEnabled(key));
        }

        [InlineData(DiagnosticEventKeys.MiddlewareStart)]
        [InlineData(DiagnosticEventKeys.MiddlewareFailure)]
        [InlineData(DiagnosticEventKeys.MiddlewareSuccess)]
        [InlineData(DiagnosticEventKeys.OperationFailure)]
        [InlineData(DiagnosticEventKeys.OperationStart)]
        [InlineData(DiagnosticEventKeys.OperationSuccess)]
        [InlineData(DiagnosticEventKeys.RequestFailure)]
        [InlineData(DiagnosticEventKeys.RequestStart)]
        [InlineData(DiagnosticEventKeys.RequestSuccess)]
        [Theory]
        public void IsEnabled_NotSubscribedByDefault(string key)
        {
            var tracer = new TestTracer();
            Assert.False(tracer.IsEnabled(key));
        }

        private class TestTracer : DiagnosticTracerBase
        {
        }
    }
}
