using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;
using Autofac.Builder;
using AutofacContrib.DynamicProxy2;
using Autofac.Component;
using Castle.Core.Interceptor;

namespace AutofacContrib.Tests.DynamicProxy2
{
    [TestFixture]
    public class ExtendedPropertyInterceptorProviderFixture
    {
        [Test]
        public void ProvidesInterceptors()
        {
            var interceptorServices = new Service[] { new NamedService("ns1"), new TypedService(typeof(string)) };

            var descr = new Descriptor(new UniqueService(), new Service[0], typeof(object),
                new Dictionary<string, object>(){{ExtendedPropertyInterceptorProvider.InterceptorsPropertyName, interceptorServices}});

            var prov = new ExtendedPropertyInterceptorProvider();
            var result = prov.GetInterceptorServices(descr);

            Assert.AreEqual(interceptorServices.Length, result.Count());
            foreach (var s in interceptorServices)
                Assert.IsTrue(result.Contains(s));
        }

        [Test]
        public void NoInterceptorsIfPropertyMissing()
        {
            var descr = new Descriptor(new UniqueService(), new Service[0], typeof(object));

            var prov = new ExtendedPropertyInterceptorProvider();
            var result = prov.GetInterceptorServices(descr);

            Assert.IsFalse(result.Any());
        }

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

            public virtual int GetVisitCount()
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
            builder.Register<CustomerService>().As<ICustomerService>().ContainerScoped().InterceptedBy(typeof(AddOneInterceptor));
            builder.Register<AddOneInterceptor>();
            builder.RegisterModule(new FlexibleInterceptionModule());
            var container = builder.Build();
            var cs = container.Resolve<ICustomerService>();
            Assert.AreEqual(11, cs.GetVisitCount());
        }
    }
}
