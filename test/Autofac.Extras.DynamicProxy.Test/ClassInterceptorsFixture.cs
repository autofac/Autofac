using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class ClassInterceptorsFixture
    {
        [Fact]
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
            Assert.Equal(i + 1, got);
        }

        [Fact]
        public void InterceptsReflectionBasedComponent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var container = builder.Build();
            var i = 10;
            var c = container.Resolve<C>(TypedParameter.From(i));
            var got = c.GetI();
            Assert.Equal(i + 1, got);
        }

        [Intercept(typeof(AddOneInterceptor))]
        public class C
        {
            public C(int i)
            {
                I = i;
            }

            public int I { get; set; }

            public virtual int GetI()
            {
                return I;
            }
        }

        public class D
        {
            public D(int i)
            {
                I = i;
            }

            public int I { get; set; }

            public virtual int GetI()
            {
                return I;
            }
        }

        private class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetI")
                {
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
                }
            }
        }
    }
}
