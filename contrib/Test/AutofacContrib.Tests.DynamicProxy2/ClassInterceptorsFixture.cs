using Autofac;
using Autofac.Builder;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using NUnit.Framework;
using System;

namespace AutofacContrib.Tests.DynamicProxy2
{
    [TestFixture]
    public class ClassInterceptorsFixture
    {
        [Intercept(typeof(AddOneInterceptor))]
        public class C
        {
            public int I { get; set; }

            public C(int i)
            {
                I = i;
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
        public void InterceptsReflectionBasedComponent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var container = builder.Build();
            var i = 10;
            var c = container.Resolve<C>(TypedParameter.From(i));
            var got = c.GetI();
            Assert.AreEqual(i + 1, got);
        }
    }
}
