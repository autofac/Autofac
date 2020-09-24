// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Diagnostics;
using Xunit;

namespace Autofac.Specification.Test.Diagnostics
{
    public class DiagnosticTracerBaseTests
    {
        [Fact]
        public void Enable_Disable()
        {
            var tracer = new TestTracer();
            Assert.False(tracer.IsEnabled("TestEvent"));
            tracer.Enable("TestEvent");
            Assert.True(tracer.IsEnabled("TestEvent"));
            tracer.Disable("TestEvent");
            Assert.False(tracer.IsEnabled("TestEvent"));
        }

        private class TestTracer : DiagnosticTracerBase
        {
        }
    }
}
