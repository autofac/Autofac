using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac.Extras.DynamicProxy2;
using Autofac.Integration.Wcf;
using Castle.DynamicProxy;
using NUnit.Framework;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class WcfInterceptorFixture
    {
        private static readonly Uri TestServiceAddress = new Uri("http://localhost:80/Temporary_Listen_Addresses/ITestService");

        [Test(Description = "Issue 361: WCF service client code should allow interception to occur.")]
        [Ignore("Issue #361")]
        public void ServiceClientInterceptionIsPossible()
        {
            /* The root cause of Issue #361 is that when you call CreateChannel on
             * the ChannelFactory<T> you end up getting a System.Runtime.Remoting.Proxies.RealProxy
             * object that, when you call GetType, reports itself as being a concrete instance
             * of the service interface. That is...
             * typeof(ITestService) == proxy.GetType();
             * In any other circumstance, GetType would return a concrete class because you
             * can't have an instance of an interface - you have an instance of a concrete type
             * that implements an interface.
             * 
             * This causes all nature of trouble.
             * 
             * You can generate a DynamicProxy around the RealProxy without issue, but
             * Autofac itself will complain during resolution that "this" when resolved
             * can't actually be an instance of an interface. The problem appears to be
             * fairly deeply rooted and very special-case around service clients. */

            // Build the service-side container
            var sb = new ContainerBuilder();
            sb.RegisterType<TestService>().As<ITestService>();

            // Build the client-side container with interception
            // around the client proxy. Issue 361 was that there
            // seemed to be trouble around getting this to work.
            var cb = new ContainerBuilder();
            cb.RegisterType<TestServiceInterceptor>();
            cb.Register(c => new ChannelFactory<ITestService>(new BasicHttpBinding(), new EndpointAddress(TestServiceAddress))).SingleInstance();
            cb
                .Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(TestServiceInterceptor))
                .UseWcfSafeRelease();

            using (var sc = sb.Build())
            {
                // Start the self-hosted test service
                var host = CreateTestServiceHost(sc);
                host.Open();
                try
                {
                    using (var cc = cb.Build())
                    {
                        // Make a call through the client to the service -
                        // it should be intercepted.
                        var client = cc.Resolve<ITestService>();
                        Assert.AreEqual("interceptor", client.DoWork(), "The call through the client proxy to the service was not intercepted.");
                    }
                }
                finally
                {
                    host.Close();
                }
            }
        }

        private static ServiceHost CreateTestServiceHost(ILifetimeScope container)
        {
            var host = new ServiceHost(typeof(TestService), TestServiceAddress);
            host.AddServiceEndpoint(typeof(ITestService), new BasicHttpBinding(), "");
            host.AddDependencyInjectionBehavior<ITestService>(container);
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = TestServiceAddress });
            return host;
        }

        [ServiceContract]
        public interface ITestService
        {
            [OperationContract]
            string DoWork();
        }

        public class TestService : ITestService
        {
            public string DoWork()
            {
                return "service";
            }
        }

        public class TestServiceInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.Name == "DoWork")
                {
                    invocation.ReturnValue = "interceptor";
                }
                else
                {
                    invocation.Proceed();
                }
            }
        }
    }
}
