using System;
using NUnit.Framework;
using System.Linq;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class ProvidedInstanceActivatorTests
    {
        [Test]
        public void NullIsNotAValidInstance()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new ProvidedInstanceActivator(null);
            });
        }

        [Test]
        public void WhenInitializedWithInstance_ThatInstanceIsReturnedFromActivateInstance()
        {
            object instance = new object();

            ProvidedInstanceActivator target = new ProvidedInstanceActivator(instance);
            
            var actual = target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);

            Assert.AreSame(instance, actual);
        }

        [Test]
        public void ActivatingAProvidedInstanceTwice_RaisesException()
        {
            object instance = new object();

            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(instance);

            target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);

            Assertions.AssertThrows<InvalidOperationException>(delegate
            {
                target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);
            });
        }
    }
}
