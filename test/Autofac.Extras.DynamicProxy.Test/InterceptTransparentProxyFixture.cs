using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac.Core;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class InterceptTransparentProxyFixture
    {
        private static readonly Uri TestServiceAddress = new Uri("http://localhost:80/Temporary_Listen_Addresses/ITestService");

        [ServiceContract]
        public interface ITestService
        {
            [OperationContract]
            string DoWork();
        }

        [Fact]
        public void ProxyMustImplementAdditionalInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => CreateChannelFactory()).SingleInstance();
            builder.Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .SingleInstance()
                .InterceptTransparentProxy(typeof(ICloneable), typeof(IFormattable));

            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ITestService>());

            Assert.Contains(typeof(ICloneable).FullName, exception.Message);
            Assert.Contains(typeof(IFormattable).FullName, exception.Message);
        }

        [Fact]
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
                .InterceptedBy(typeof(TestServiceInterceptor));

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
                        Assert.Equal("interceptor", client.DoWork());
                    }
                }
                finally
                {
                    host.Close();
                }
            }
        }

        [Fact]
        public void ServiceMustBeInterface()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InterceptTransparentProxy();
            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => container.Resolve<object>());
        }

        [Fact]
        public void ServiceMustBeTransparentProxy()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).As<ITestService>().InterceptTransparentProxy();
            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ITestService>());

            Assert.Contains(typeof(object).FullName, exception.Message);
        }

        private static ChannelFactory<ITestService> CreateChannelFactory()
        {
            return new ChannelFactory<ITestService>(new BasicHttpBinding(), new EndpointAddress(TestServiceAddress));
        }

        private static ServiceHost CreateTestServiceHost(ILifetimeScope container)
        {
            var host = new ServiceHost(typeof(TestService), TestServiceAddress);
            host.AddServiceEndpoint(typeof(ITestService), new BasicHttpBinding(), "");
            // host.AddDependencyInjectionBehavior<ITestService>(container);
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = TestServiceAddress });
            return host;
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
