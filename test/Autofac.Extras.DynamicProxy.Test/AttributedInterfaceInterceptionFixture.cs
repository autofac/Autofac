using System;
using Autofac.Core;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class AttributedInterfaceInterceptionFixture
    {
        [Intercept(typeof(AddOneInterceptor))]
        public interface IHasI
        {
            int GetI();
        }

        [Fact]
        public void DetectsNonInterfaceServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var c = builder.Build();
            var dx = Assert.Throws<DependencyResolutionException>(() => c.Resolve<C>());
            Assert.IsType<InvalidOperationException>(dx.InnerException);
        }

        [Fact]
        public void FindsInterceptionAttributeOnExpressionComponent()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new C()).As<IHasI>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>();

            Assert.Equal(11, cpt.GetI()); // proxied
        }

        [Fact]
        public void FindsInterceptionAttributeOnReflectionComponent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().As<IHasI>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>();

            Assert.Equal(11, cpt.GetI()); // proxied
        }

        public class C : IHasI
        {
            public C()
            {
                I = 10;
            }

            public int I { get; private set; }

            public int GetI()
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
