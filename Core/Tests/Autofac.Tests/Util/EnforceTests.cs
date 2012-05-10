using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Util;

namespace Autofac.Tests.Util
{
    [TestFixture]
    public class EnforceTests
    {
        [Test]
        public void FindsEmptyElementInList()
        {
            Assertions.AssertThrows<ArgumentException>(() =>
                Enforce.ArgumentElementNotNull(new object[] { null }, "arg"));
        }
    }
}
