using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ModuleFixture
    {
        class ObjectModule : Module
        {
            protected override void Load()
            {
                this.Register(new object());
            }
        }

        [Test]
        public void LoadsRegistrations()
        {
            var container = new Container();
            new ObjectModule().Configure(container);
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullContainer()
        {
            new ObjectModule().Configure(null);
        }
    }
}
