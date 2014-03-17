using Castle.DynamicProxy;
using NUnit.Framework;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class InterceptorsChosenByMetadataFixture
    {
        public interface ICustomerService
        {
            int GetVisitCount();
			int GetUniqueVisitorCount();
        }

        public class CustomerService : ICustomerService
        {
            int VisitCount { get; set; }
			int UniqueVisitorCount { get; set; }

            public CustomerService()
            {
                VisitCount = 10;
				UniqueVisitorCount = 6;
            }

            public int GetVisitCount()
            {
                return VisitCount;
            }

			public int GetUniqueVisitorCount()
			{
				return UniqueVisitorCount;
			}
        }

        class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name.StartsWith("Get"))
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
            }
        }

		class AddTenInterceptor : IInterceptor
		{
			public void Intercept(IInvocation invocation)
			{
				invocation.Proceed();
				if (invocation.Method.Name == "GetUniqueVisitorCount")
					invocation.ReturnValue = 10 + (int)invocation.ReturnValue;
			}
		}

		class InterceptOnlyUnique : IProxyGenerationHook
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

		class MyInterceptorSelector : IInterceptorSelector
		{
			public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
			{
				return method.Name == "GetVisitCount"
					? interceptors.OfType<AddOneInterceptor>().ToArray<IInterceptor>()
					: interceptors.OfType<AddTenInterceptor>().ToArray<IInterceptor>();
			}
		}

        [Test]
        public void InterceptsWhenUsingExtendedPropertyAndType()
		{
            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AddOneInterceptor));
            builder.RegisterType<AddOneInterceptor>();
            var container = builder.Build();
            var cs = container.Resolve<ICustomerService>();
            Assert.AreEqual(11, cs.GetVisitCount());
        }

		[Test]
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

			Assert.IsNotNull(dict);

			dict.Add(1, 2);

			Assert.AreEqual(2, dict[1]);

			dict.Clear();

			Assert.IsEmpty(dict);
		}

		[Test]
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

			Assert.AreEqual(10, cpt.GetVisitCount());
			Assert.AreEqual(17, cpt.GetUniqueVisitorCount());
		}

		[Test]
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

			Assert.AreEqual(11, cpt.GetVisitCount());
			Assert.AreEqual(16, cpt.GetUniqueVisitorCount());
		}

    }
}
