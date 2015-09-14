using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class InterceptorsChosenByMetadataWithOptionsFixture
    {
        public interface ICustomerService
        {
            int GetUniqueVisitorCount();

            int GetVisitCount();
        }

        [Fact]
        public void CanInterceptMethodsWithSpecificInterceptors()
        {
            var options = new ProxyGenerationOptions { Selector = new MyInterceptorSelector() };

            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors(options)
                .InterceptedBy(typeof(AddOneInterceptor), typeof(AddTenInterceptor));
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var cpt = builder.Build().Resolve<ICustomerService>();

            Assert.Equal(11, cpt.GetVisitCount());
            Assert.Equal(16, cpt.GetUniqueVisitorCount());
        }

        [Fact]
        public void CanInterceptOnlySpecificMethods()
        {
            var options = new ProxyGenerationOptions(new InterceptOnlyUnique());

            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors(options)
                .InterceptedBy(typeof(AddOneInterceptor), typeof(AddTenInterceptor));
            builder.RegisterType<AddOneInterceptor>();
            builder.RegisterType<AddTenInterceptor>();
            var cpt = builder.Build().Resolve<ICustomerService>();

            Assert.Equal(10, cpt.GetVisitCount());
            Assert.Equal(17, cpt.GetUniqueVisitorCount());
        }

        [Fact]
        public void InterceptsWithMixinWhenUsingExtendedPropertyAndType()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new Dictionary<int, int>());

            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors(options);
            var container = builder.Build();

            var cs = container.Resolve<ICustomerService>();
            var dict = cs as IDictionary<int, int>;

            Assert.NotNull(dict);

            dict.Add(1, 2);

            Assert.Equal(2, dict[1]);

            dict.Clear();

            Assert.Empty(dict);
        }

        public class CustomerService : ICustomerService
        {
            public CustomerService()
            {
                VisitCount = 10;
                UniqueVisitorCount = 6;
            }

            private int UniqueVisitorCount { get; set; }

            private int VisitCount { get; set; }

            public int GetUniqueVisitorCount()
            {
                return UniqueVisitorCount;
            }

            public int GetVisitCount()
            {
                return VisitCount;
            }
        }

        private class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name.StartsWith("Get"))
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
                if (invocation.Method.Name == "GetUniqueVisitorCount")
                {
                    invocation.ReturnValue = 10 + (int)invocation.ReturnValue;
                }
            }
        }

        private class InterceptOnlyUnique : IProxyGenerationHook
        {
            public void MethodsInspected()
            {
            }

            public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
            {
            }

            public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                return methodInfo.Name.Equals("GetUniqueVisitorCount");
            }
        }

        private class MyInterceptorSelector : IInterceptorSelector
        {
            public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
            {
                return method.Name == "GetVisitCount"
                    ? interceptors.OfType<AddOneInterceptor>().ToArray<IInterceptor>()
                    : interceptors.OfType<AddTenInterceptor>().ToArray<IInterceptor>();
            }
        }
    }
}
