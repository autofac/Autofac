// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class OrderingTests
    {
        [Fact]
        public void LastInWins()
        {
            var cb = new ContainerBuilder();
            var inst1 = new object();
            var inst2 = new object();
            cb.RegisterInstance(inst1);
            cb.RegisterInstance(inst2);
            var c = cb.Build();
            Assert.Same(inst2, c.Resolve<object>());
        }
    }
}
