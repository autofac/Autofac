// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Util;
using Xunit;

namespace Autofac.Test.Util
{
    public class DelegateExtensionsTests
    {
        public class WithTwoInvokes
        {
            public void Invoke()
            {
            }

            public void Invoke(string s)
            {
            }
        }

        // Issue 179
        [Fact]
        public void TypeWithTwoInvokeMethodsIsNotADelegate()
        {
            Assert.False(typeof(WithTwoInvokes).IsDelegate());
        }
    }
}
