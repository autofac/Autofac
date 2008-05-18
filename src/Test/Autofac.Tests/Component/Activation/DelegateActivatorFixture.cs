using System;
using Autofac.Component.Activation;
using NUnit.Framework;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class DelegateActivatorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructNull()
        {
            new DelegateActivator(null);
        }

        [Test]
        public void ActivateInstance()
        {
            object instance = new object();

            DelegateActivator target =
                new DelegateActivator((c, p) => instance);

            Assert.AreSame(instance, target.ActivateInstance(new Container(), ActivationParameters.Empty));
        }
	}
}
