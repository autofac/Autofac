using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class ClassInterceptorsWithOptionsFixture
    {
        [Fact]
        public void CanCreateMixinWithClassInterceptors()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new Dictionary<int, int>());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

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
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

            Assert.Equal(i + 1, cpt.GetI());
            Assert.Equal(i + 10, cpt.GetJ());
        }

        [Fact]
        public void CanInterceptOnlySpecificMethods()
        {
            var options = new ProxyGenerationOptions(new InterceptOnlyJ());

            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableClassInterceptors(options);
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var container = builder.Build();
            var i = 10;
            var cpt = container.Resolve<C>(TypedParameter.From(i));

            Assert.Equal(i, cpt.GetI());
            Assert.Equal(i + 11, cpt.GetJ());
        }

        [Intercept(typeof(AddOneInterceptor))]
        [Intercept(typeof(AddTenInterceptor))]
        public class C
        {
            public C(int i)
            {
                I = J = i;
            }

            public int I { get; set; }

            public int J { get; set; }

            public virtual int GetI()
            {
                return I;
            }

            public virtual int GetJ()
            {
                return J;
            }
        }

        public class D
        {
            public D(int i)
            {
                I = J = i;
            }

            public int I { get; set; }

            public int J { get; set; }

            public virtual int GetI()
            {
                return I;
            }

            public virtual int GetJ()
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
                var result = method.Name == "GetI"
                    ? interceptors.OfType<AddOneInterceptor>().ToArray<IInterceptor>()
                    : interceptors.OfType<AddTenInterceptor>().ToArray<IInterceptor>();

                if (result.Length == 0)
                {
                    throw new InvalidOperationException("No interceptors for method " + method.Name);
                }
                return result;
            }
        }
    }
}
