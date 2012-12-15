using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac.Core;
using Autofac.Extras.DynamicProxy2;
using Autofac.Integration.Wcf;
using Castle.DynamicProxy;
using NUnit.Framework;

namespace Autofac.Extras.Tests.DynamicProxy2
{
    [TestFixture]
    public class InterceptTransparentProxyFixture
    {
        private static readonly Uri TestServiceAddress = new Uri("http://localhost:80/Temporary_Listen_Addresses/ITestService");

        [Test(Description = "The service being intercepted must be registered as an interface.")]
        public void ServiceMustBeInterface()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InterceptTransparentProxy();
            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
        }

        [Test(Description = "The instance being intercepted must be a transparent proxy.")]
        public void ServiceMustBeTransparentProxy()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).As<ITestService>().InterceptTransparentProxy();
            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ITestService>());

            Assert.That(exception.Message, Is.StringContaining(typeof(object).FullName));
        }

        [Test(Description = "The instance must implement the additional interfaces provided.")]
        public void ProxyMustImplementAdditionalInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => CreateChannelFactory()).SingleInstance();
            builder.Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .SingleInstance()
                .InterceptTransparentProxy(typeof(ICloneable), typeof(IFormattable));
            
            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ITestService>());

            Assert.That(exception.Message, Is.StringContaining(typeof(ICloneable).FullName));
            Assert.That(exception.Message, Is.StringContaining(typeof(IFormattable).FullName));
        }

        [Test(Description = "Issue 361: WCF service client code should allow interception to occur.")]
        public void ServiceClientInterceptionIsPossible()
        {
            // Build the service-side container
            var sb = new ContainerBuilder();
            sb.RegisterType<TestService>().As<ITestService>();

            // Build the client-side container with interception
            // around the client proxy. Issue 361 was that there
            // seemed to be trouble around getting this to work.
            var cb = new ContainerBuilder();
            cb.RegisterType<TestServiceInterceptor>();
            cb.Register(c => CreateChannelFactory()).SingleInstance();
            cb
                .Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .InterceptTransparentProxy(typeof(IClientChannel))
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

        static ChannelFactory<ITestService> CreateChannelFactory()
        {
            return new ChannelFactory<ITestService>(new BasicHttpBinding(), new EndpointAddress(TestServiceAddress));
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
