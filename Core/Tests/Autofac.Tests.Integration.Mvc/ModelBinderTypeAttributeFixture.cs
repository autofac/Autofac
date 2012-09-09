using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class ModelBinderTypeAttributeFixture
    {
        [Test]
        public void NullTargetTypeThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ModelBinderTypeAttribute((Type)null));
        }

        [Test]
        public void NullTargetTypesThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ModelBinderTypeAttribute((Type[])null));
        }
    }
}
