using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using Autofac.Builder;

namespace AutofacContrib.Tests.DynamicProxy2
{
    [TestFixture]
    public class FlexibleInterceptionModuleFixture
    {
        public interface IHasI
        {
            int GetI();
        }

        [Intercept(typeof(AddOneInterceptor))]
        public class C : IHasI
        {
            public int I { get; private set; }

            public C()
            {
                I = 10;
            }

            public virtual int GetI()
            {
                return I;
            }
        }

        class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetI")
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
            }
        }

        [Test]
        public void AttachesToReflectiveComponentWithSubclassProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<C>();

            Assert.AreEqual(11, cpt.GetI()); // proxied
            Assert.IsTrue(typeof(C).IsAssignableFrom(cpt.GetType()));
        }

        [Test]
        public void AttachesToExpressionComponentWithServiceProxy()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new C()).As<IHasI>().EnableInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>();

            Assert.AreEqual(11, cpt.GetI()); // proxied
            Assert.IsFalse(typeof(C).IsAssignableFrom(cpt.GetType()));
        }

        [Test]
        public void DynamicallyAttachesIfNoTypedServices()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new C()).Named<IHasI>("cpt").EnableInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>("cpt");

            Assert.AreEqual(11, cpt.GetI()); // proxied
            Assert.IsFalse(typeof(C).IsAssignableFrom(cpt.GetType()));
        }
    }
}
