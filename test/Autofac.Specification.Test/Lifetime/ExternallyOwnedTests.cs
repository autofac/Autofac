// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class ExternallyOwnedTests
    {
        [Fact]
        public void RootInstancesNotDisposedOnContainerDisposal()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().ExternallyOwned();
            var c = cb.Build();
            var a1 = c.Resolve<A>();
            var a2 = c.Resolve<A>();
            c.Dispose();

            Assert.False(a1.IsDisposed);
            Assert.False(a2.IsDisposed);
        }

        private class A : DisposeTracker
        {
        }
    }
}
