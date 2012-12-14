using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Extras.DynamicProxy2;
using Castle.DynamicProxy;
using NUnit.Framework;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class InterfaceInterceptorsFixture
    {
        [Test(Description = "Interception should not be able to occur against internal interfaces.")]
        public void DoesNotInterceptInternalInterfaces()
        {
            // DynamicProxy2 only supports visible interfaces so internal won't work.
            var builder = new ContainerBuilder();
            builder.RegisterType<StringMethodInterceptor>();
            builder
                .RegisterType<Interceptable>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(StringMethodInterceptor))
                .As<IInternalInterface>();
            var container = builder.Build();
            var dre = Assert.Throws<DependencyResolutionException>(() => container.Resolve<IInternalInterface>());
            Assert.IsInstanceOf<InvalidOperationException>(dre.InnerException, "The inner exception should explain about public interfaces being required.");
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
