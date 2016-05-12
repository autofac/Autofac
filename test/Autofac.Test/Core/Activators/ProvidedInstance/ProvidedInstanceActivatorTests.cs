using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Xunit;

namespace Autofac.Test.Component.Activation
{
    public class ProvidedInstanceActivatorTests
    {
        [Fact]
        public void NullIsNotAValidInstance()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ProvidedInstanceActivator(null);
            });
        }

        [Fact]
        public void WhenInitializedWithInstance_ThatInstanceIsReturnedFromActivateInstance()
        {
            object instance = new object();

            ProvidedInstanceActivator target = new ProvidedInstanceActivator(instance);

            var actual = target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);

            Assert.Same(instance, actual);
        }

        [Fact]
        public void ActivatingAProvidedInstanceTwice_RaisesException()
        {
            object instance = new object();

            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(instance);

            target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);

            Assert.Throws<InvalidOperationException>(delegate
            {
                target.ActivateInstance(Factory.EmptyContainer, Factory.NoParameters);
            });
        }
    }
}
