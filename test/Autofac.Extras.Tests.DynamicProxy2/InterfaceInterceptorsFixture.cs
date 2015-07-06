using System;
using System.Runtime.CompilerServices;
using Autofac.Extras.DynamicProxy2;
using Autofac.Extras.Tests.DynamicProxy2.SatelliteAssembly;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using NUnit.Framework;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class InterfaceInterceptorsFixture
    {
        [Test(Description = "Interception should be able to occur against internal interfaces when InternalVisibleTo attribute set.")]
        public void InterceptsInternalInterfacesWithInternalsVisibleToDynamicProxyGenAssembly2()
        {
            var internalsAttribute = typeof(InterfaceInterceptorsFixture).Assembly.GetAttribute<InternalsVisibleToAttribute>();
            Assert.That(internalsAttribute.AssemblyName, Is.StringStarting("DynamicProxyGenAssembly2"));

            var builder = new ContainerBuilder();
            builder.RegisterType<StringMethodInterceptor>();
            builder
                .RegisterType<Interceptable>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(StringMethodInterceptor))
                .As<IInternalInterface>();
            var container = builder.Build();
            var obj = container.Resolve<IInternalInterface>();
            Assert.AreEqual("intercepted-InternalMethod", obj.InternalMethod(), "The interface method should have been intercepted.");
        }

        [Test(Description = "Interception should be able to occur against public interfaces.")]
        public void InterceptsPublicInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StringMethodInterceptor>();
            builder
                .RegisterType<Interceptable>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(StringMethodInterceptor))
                .As<IPublicInterface>();
            var container = builder.Build();
            var obj = container.Resolve<IPublicInterface>();
            Assert.AreEqual("intercepted-PublicMethod", obj.PublicMethod(), "The interface method should have been intercepted.");
        }
		
        [Test(Description = "Interception should be able to occur against public interfaces.")]
        public void InterceptsPublicInterfacesSatelliteAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StringMethodInterceptor>();
            builder
                .RegisterType<InterceptablePublicSatellite>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(StringMethodInterceptor))
                .As<IPublicInterfaceSatellite>();
            var container = builder.Build();
            var obj = container.Resolve<IPublicInterfaceSatellite>();
            Assert.AreEqual("intercepted-PublicMethod", obj.PublicMethod(), "The interface method should have been intercepted.");
        }

        public class Interceptable : IPublicInterface, IInternalInterface
        {
            public string PublicMethod()
            {
                throw new NotImplementedException();
            }

            public string InternalMethod()
            {
                throw new NotImplementedException();
            }
        }

        public interface IPublicInterface
        {
            string PublicMethod();
        }

        internal interface IInternalInterface
        {
            string InternalMethod();
        }

        private class StringMethodInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.ReturnType == typeof(string))
                {
                    invocation.ReturnValue = "intercepted-" + invocation.Method.Name;
                }
                else
                {
                    invocation.Proceed();
                }
            }
        }

    }
}
