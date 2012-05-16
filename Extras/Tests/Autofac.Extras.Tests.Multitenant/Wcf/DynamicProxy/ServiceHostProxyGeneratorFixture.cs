using System;
using System.ServiceModel;
using AutofacContrib.Multitenant.Wcf.DynamicProxy;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant.Wcf.DynamicProxy
{
    [TestFixture]
    public class ServiceHostProxyGeneratorFixture
    {
        [Test(Description = "Verifies that the proxy builder used is the special service host proxy builder.")]
        public void Ctor_ProxyBuilderIsServiceHostProxyBuilder()
        {
            var generator = new ServiceHostProxyGenerator();
            Assert.IsInstanceOf<ServiceHostProxyBuilder>(generator.ProxyBuilder);
        }

        [Test(Description = "Creates a proxy to a service interface and verifies it can be hosted.")]
        public void CreateWcfProxy_CustomProxyTypeCanBeHosted()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new ServiceImplementation();
            Type interfaceToProxy = typeof(IServiceContract);
            var proxy = generator.CreateWcfProxy(interfaceToProxy, target);
            Assert.DoesNotThrow(() =>
            {
                new ServiceHost(proxy.GetType(), new Uri("http://localhost:22111/Foo.svc"));
            });
        }

        [Test(Description = "Attempts to create a proxy to a type not an interface.")]
        public void CreateWcfProxy_InterfaceToProxyNotInterface()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new ServiceImplementation();
            Type interfaceToProxy = typeof(ServiceImplementation);
            Assert.Throws<ArgumentException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        [Test(Description = "Attempts to create a proxy to an interface that isn't a service contract.")]
        public void CreateWcfProxy_InterfaceToProxyNotServiceContract()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new NotAServiceImplementation();
            Type interfaceToProxy = typeof(INotAServiceContract);
            Assert.Throws<ArgumentException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        [Test(Description = "Attempts to create a proxy to an interface that's generic.")]
        public void CreateWcfProxy_InterfaceToProxyIsGeneric()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new ServiceImplementation();
            Type interfaceToProxy = typeof(IServiceContractGeneric<>);
            Assert.Throws<ArgumentException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        [Test(Description = "Attempts to create a proxy to a null interface type.")]
        public void CreateWcfProxy_NullInterface()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new ServiceImplementation();
            Type interfaceToProxy = null;
            Assert.Throws<ArgumentNullException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        [Test(Description = "Attempts to create a proxy to a null target.")]
        public void CreateWcfProxy_NullTarget()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = null;
            Type interfaceToProxy = typeof(IServiceContract);
            Assert.Throws<ArgumentNullException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        [Test(Description = "Attempts to create a proxy to a target that does not implement the interface.")]
        public void CreateWcfProxy_TargetDoesNotImplementInterface()
        {
            var generator = new ServiceHostProxyGenerator();
            object target = new NotAServiceImplementation();
            Type interfaceToProxy = typeof(IServiceContract);
            Assert.Throws<ArgumentException>(() => generator.CreateWcfProxy(interfaceToProxy, target));
        }

        public interface INotAServiceContract
        {
            // Has to be public or Castle.DynamicProxy can't make a proxy.
        }

        [ServiceContract]
        public interface IServiceContract
        {
            // Has to be public or Castle.DynamicProxy can't make a proxy.
            void MethodToProxy();
        }

        [ServiceContract]
        public interface IServiceContractGeneric<T>
        {
            // Has to be public or Castle.DynamicProxy can't make a proxy.
            void MethodToProxy();
        }

        private class ServiceImplementation : IServiceContract
        {
            public bool ProxyMethodCalled { get; set; }
            public void MethodToProxy()
            {
                this.ProxyMethodCalled = true;
            }
        }

        private class NotAServiceImplementation : INotAServiceContract
        {
        }
    }
}
