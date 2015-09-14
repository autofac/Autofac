using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class AttributedInterfaceInterceptionWithOptionsFixture
    {
        [Intercept(typeof(AddOneInterceptor))]
        [Intercept(typeof(AddTenInterceptor))]
        public interface IHasIAndJ
        {
            int GetI();

            int GetJ();
        }

        [Fact]
        public void CanCreateMixinWithAttributeInterceptors()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new Dictionary<int, int>());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().As<IHasIAndJ>().EnableInterfaceInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var cpt = builder.Build().Resolve<IHasIAndJ>();

            var dict = cpt as IDictionary<int, int>;

            Assert.NotNull(dict);

            dict.Add(1, 2);

            Assert.Equal(2, dict[1]);

            dict.Clear();

            Assert.Empty(dict);
        }

        [Fact]
        public void CanInterceptMethodsWithSpecificInterceptors()
        {
            var options = new ProxyGenerationOptions { Selector = new MyInterceptorSelector() };

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().As<IHasIAndJ>().EnableInterfaceInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var cpt = builder.Build().Resolve<IHasIAndJ>();

            Assert.Equal(11, cpt.GetI());
            Assert.Equal(20, cpt.GetJ());
        }

        [Fact]
        public void CanInterceptOnlySpecificMethods()
        {
            var options = new ProxyGenerationOptions(new InterceptOnlyJ());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().As<IHasIAndJ>().EnableInterfaceInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var cpt = builder.Build().Resolve<IHasIAndJ>();

            Assert.Equal(10, cpt.GetI());
            Assert.Equal(21, cpt.GetJ());
        }

        public class C : IHasIAndJ
        {
            public C()
            {
                I = J = 10;
            }

            public int I { get; private set; }

            public int J { get; private set; }

            public int GetI()
            {
                return I;
            }

            public int GetJ()
            {
                return J;
            }
        }

        private class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetI" || invocation.Method.Name == "GetJ")
                {
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
                }
            }
        }

        private class AddTenInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetJ")
                {
                    invocation.ReturnValue = 10 + (int)invocation.ReturnValue;
                }
            }
        }

        private class InterceptOnlyJ : IProxyGenerationHook
        {
            public void MethodsInspected()
            {
            }

            public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
            {
            }

            public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                return methodInfo.Name.Equals("GetJ");
            }
        }

        private class MyInterceptorSelector : IInterceptorSelector
        {
            public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
            {
                return method.Name == "GetI"
                    ? interceptors.OfType<AddOneInterceptor>().ToArray<IInterceptor>()
                    : interceptors.OfType<AddTenInterceptor>().ToArray<IInterceptor>();
            }
        }
    }
}
