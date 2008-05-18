using System;
using Autofac.Component.Activation;
using NUnit.Framework;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class ProvidedInstanceActivatorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructNull()
        {
            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(null);
        }

        [Test]
        public void ActivateInstance()
        {
            object instance = new object();

            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(instance);

            Assert.AreSame(instance, target.ActivateInstance(new Container(), ActivationParameters.Empty));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ActivateInstanceTwice()
        {
            object instance = new object();

            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(instance);

            IContext container = new Container();

            target.ActivateInstance(container, ActivationParameters.Empty);
            target.ActivateInstance(container, ActivationParameters.Empty);
        }
    }
}
