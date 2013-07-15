using Autofac;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy2;
using Castle.DynamicProxy;
using NUnit.Framework;
using System;

namespace Autofac.Extras.Tests.DynamicProxy2
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

        public class D
        {
            public int I { get; set; }

            public D(int i)
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

        [Test]
        public void InterceptorCanBeWiredUsingInterceptedBy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<D>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(AddOneInterceptor));
            builder.RegisterType<AddOneInterceptor>();
            var container = builder.Build();
            var i = 10;
            var c = container.Resolve<D>(TypedParameter.From(i));
            var got = c.GetI();
            Assert.AreEqual(i + 1, got);
        }
    }
}
