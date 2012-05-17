using System;
using Autofac.Extras.Multitenant.Wcf.DynamicProxy;
using Castle.DynamicProxy.Generators;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf.DynamicProxy
{
    [TestFixture]
    public class ServiceHostProxyBuilderFixture
    {
        [Test(Description = "Builds a proxy type.")]
        public void CreateWcfProxyType_BuildsProxyType()
        {
            var builder = new ServiceHostProxyBuilder();
            var type = builder.CreateWcfProxyType(typeof(ValidType));
            Assert.IsNotNull(type, "The generated type should not be null.");
        }

        [Test(Description = "Attempts to build a proxy type based on a null input.")]
        public void CreateWcfProxyType_NullTypeToProxy()
        {
            var builder = new ServiceHostProxyBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.CreateWcfProxyType(null));
        }

        [Test(Description = "Attempts to build a proxy type based on a generic type.")]
        public void CreateWcfProxyType_TypeIsGeneric()
        {
            var builder = new ServiceHostProxyBuilder();
            Assert.Throws<GeneratorException>(() => builder.CreateWcfProxyType(typeof(GenericType<>)));
        }

        [Test(Description = "Attempts to build a proxy type based on a non-public type.")]
        public void CreateWcfProxyType_TypeNotAccessible()
        {
            var builder = new ServiceHostProxyBuilder();
            Assert.Throws<GeneratorException>(() => builder.CreateWcfProxyType(typeof(PrivateType)));
        }

        private class PrivateType
        {
        }

        public class GenericType<T>
        {
        }

        public class ValidType
        {
        }
    }
}
