using System;
using NUnit.Framework;
using System.Linq;
using Autofac.Core.Activators.Delegate;
using Autofac.Core;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class DelegateActivatorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoesNotAcceptNullDelegate()
        {
            new DelegateActivator(typeof(object), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoesNotAcceptNullType()
        {
            new DelegateActivator(null, (c, p) => new object());
        }

        [Test]
        public void ActivateInstance()
        {
            object instance = new object();

            DelegateActivator target =
                new DelegateActivator(typeof(object), (c, p) => instance);

            Assert.AreSame(instance, target.ActivateInstance(new Container(), Enumerable.Empty<Parameter>()));
        }
	}
}
