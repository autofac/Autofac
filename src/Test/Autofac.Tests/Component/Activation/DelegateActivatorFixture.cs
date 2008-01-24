using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Activation;

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
