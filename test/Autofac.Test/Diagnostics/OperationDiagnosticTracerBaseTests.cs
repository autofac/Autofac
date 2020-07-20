// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Autofac.Diagnostics;
using Xunit;

namespace Autofac.Test.Diagnostics
{
    public class OperationDiagnosticTracerBaseTests
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
        public void Ctor_DefaultSubscribesToAllEvents(string key)
        {
            var tracer = new TestTracer();
            Assert.True(tracer.IsEnabled(key));
        }

        [Fact]
        public void Ctor_SpecifySubscriptions()
        {
            var tracer = new TestTracer(new string[] { DiagnosticEventKeys.RequestStart, DiagnosticEventKeys.RequestSuccess });
            Assert.True(tracer.IsEnabled(DiagnosticEventKeys.RequestStart));
            Assert.True(tracer.IsEnabled(DiagnosticEventKeys.RequestSuccess));

            // Others shouldn't be enabled.
            Assert.False(tracer.IsEnabled(DiagnosticEventKeys.RequestFailure));
            Assert.False(tracer.IsEnabled(DiagnosticEventKeys.MiddlewareFailure));
        }

        private class TestTracer : OperationDiagnosticTracerBase<string>
        {
            public TestTracer()
                : base()
            {
            }

            public TestTracer(IEnumerable<string> subscriptions)
                : base(subscriptions)
            {
            }

            public override int OperationsInProgress => throw new System.NotImplementedException();
        }
    }
}
