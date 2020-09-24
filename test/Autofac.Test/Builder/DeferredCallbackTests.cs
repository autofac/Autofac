// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Builder;
using Xunit;

namespace Autofac.Test.Builder
{
    public class DeferredCallbackTests
    {
        [Fact]
        public void Callback_Null()
        {
            var c = new DeferredCallback(reg => { });
            Assert.Throws<ArgumentNullException>(() => { c.Callback = null; });
        }

        [Fact]
        public void Ctor_NullCallback()
        {
            Assert.Throws<ArgumentNullException>(() => new DeferredCallback(null));
        }

        [Fact]
        public void Ctor_SetsId()
        {
            var c = new DeferredCallback(reg => { });
            Assert.NotEqual(Guid.Empty, c.Id);
        }
    }
}
