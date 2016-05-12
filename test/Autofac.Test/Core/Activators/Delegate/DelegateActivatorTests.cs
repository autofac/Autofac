using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Xunit;

namespace Autofac.Test.Component.Activation
{
    public class DelegateActivatorTests
    {
        [Fact]
        public void Constructor_DoesNotAcceptNullDelegate()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateActivator(typeof(object), null));
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullType()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateActivator(null, (c, p) => new object()));
        }

        [Fact]
        public void ActivateInstance_ReturnsResultOfInvokingSuppliedDelegate()
        {
            var instance = new object();

            var target =
                new DelegateActivator(typeof(object), (c, p) => instance);

            Assert.Same(instance, target.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>()));
        }

        [Fact]
        public void WhenActivationDelegateReturnsNull_ExceptionDescribesLimitType()
        {
            var target = new DelegateActivator(typeof(string), (c, p) => null);

            var ex = Assert.Throws<DependencyResolutionException>(
                () => target.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>()));

            Assert.True(ex.Message.Contains(typeof(string).ToString()));
        }
    }
}
