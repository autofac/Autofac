// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            [SuppressMessage("CA1822", "CA1822", Justification = "Method needs to be instance for testing.")]
            public void Invoke()
            {
            }

            [SuppressMessage("CA1822", "CA1822", Justification = "Method needs to be instance for testing.")]
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
