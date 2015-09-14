using System;
using System.Runtime.CompilerServices;
using Autofac.Extras.DynamicProxy.Test.SatelliteAssembly;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class InterfaceInterceptorsFixture
    {
        public interface IPublicInterface
        {
            string PublicMethod();
        }

        internal interface IInternalInterface
        {
            string InternalMethod();
        }

        [Fact]
        public void InterceptsInternalInterfacesWithInternalsVisibleToDynamicProxyGenAssembly2()
        {
            var internalsAttribute = typeof(InterfaceInterceptorsFixture).Assembly.GetAttribute<InternalsVisibleToAttribute>();
            Assert.Contains("DynamicProxyGenAssembly2", internalsAttribute.AssemblyName);

            var builder = new ContainerBuilder();
            builder.RegisterType<StringMethodInterceptor>();
            builder
                .RegisterType<Interceptable>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(StringMethodInterceptor))
                .As<IInternalInterface>();
            var container = builder.Build();
            var obj = container.Resolve<IInternalInterface>();
            Assert.Equal("intercepted-InternalMethod", obj.InternalMethod());
        }

        [Fact]
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
            Assert.Equal("intercepted-PublicMethod", obj.PublicMethod());
        }

        [Fact]
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
            Assert.Equal("intercepted-PublicMethod", obj.PublicMethod());
        }

        public class Interceptable : IPublicInterface, IInternalInterface
        {
            public string InternalMethod()
            {
                throw new NotImplementedException();
            }

            public string PublicMethod()
            {
                throw new NotImplementedException();
            }
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
