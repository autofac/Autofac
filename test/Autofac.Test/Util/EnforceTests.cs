using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Util;
using Xunit;

namespace Autofac.Test.Util
{
    public class EnforceTests
    {
        [Fact]
        public void FindsEmptyElementInList()
        {
            Assert.Throws<ArgumentException>(() =>
                Enforce.ArgumentElementNotNull(new object[] { null }, "arg"));
        }
    }
}
