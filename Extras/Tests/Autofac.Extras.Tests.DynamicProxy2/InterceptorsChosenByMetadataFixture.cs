using Castle.DynamicProxy;
using NUnit.Framework;
using Autofac;
using Autofac.Extras.DynamicProxy2;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class InterceptorsChosenByMetadataFixture
    {
        public interface ICustomerService
        {
            int GetVisitCount();
        }

        public class CustomerService : ICustomerService
        {
            int VisitCount { get; set; }

            public CustomerService()
            {
                VisitCount = 10;
            }

            public int GetVisitCount()
            {
                return VisitCount;
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
    }
}
