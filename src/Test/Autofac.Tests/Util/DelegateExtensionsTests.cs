using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Util;
using NUnit.Framework;

namespace Autofac.Tests.Util
{
    [TestFixture]
    public class DelegateExtensionsTests
    {
        class WithTwoInvokes
        {
            public void Invoke() { }
            public void Invoke(string s) { }
        }

        // Issue 179
        [Test]
        public void TypeWithTwoInvokeMethodsIsNotADelegate()
        {
            Assert.IsFalse(typeof(WithTwoInvokes).IsDelegate());
        }
    }
}
