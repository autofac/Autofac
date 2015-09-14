using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
    public class InterceptTransparentProxyWithOptionsFixture
    {
        private static readonly Uri TestServiceAddress = new Uri("http://localhost:80/Temporary_Listen_Addresses/ITestService");

        [ServiceContract]
        public interface ITestService
        {
            [OperationContract]
            string DoOtherWork();

            [OperationContract]
            string DoWork();
        }

        [Fact]
        public void ServiceClientInterceptionIsPossibleForSpecificMethod()
        {
            var options = new ProxyGenerationOptions(new InterceptOnlyOtherWork());

            // Build the service-side container
            var sb = new ContainerBuilder();
            sb.RegisterType<TestService>().As<ITestService>();

            // Build the client-side container with interception
            // around the client proxy. Issue 361 was that there
            // seemed to be trouble around getting this to work.
            var cb = new ContainerBuilder();
            cb.RegisterType<PrependInterceptor>();
            cb.RegisterType<AppendInterceptor>();
            cb.Register(c => CreateChannelFactory()).SingleInstance();
            cb
                .Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .InterceptTransparentProxy(options, typeof(IClientChannel))
                .InterceptedBy(typeof(PrependInterceptor), typeof(AppendInterceptor));

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

                        Assert.Equal("service", client.DoWork());
                        Assert.Equal("pre-work-post", client.DoOtherWork());
                    }
                }
                finally
                {
                    host.Close();
                }
            }
        }

        [Fact]
        public void ServiceClientInterceptionIsPossibleWithMixins()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new Dictionary<int, int>());

            // Build the service-side container
            var sb = new ContainerBuilder();
            sb.RegisterType<TestService>().As<ITestService>();

            // Build the client-side container with interception
            // around the client proxy. Issue 361 was that there
            // seemed to be trouble around getting this to work.
            var cb = new ContainerBuilder();
            cb.Register(c => CreateChannelFactory()).SingleInstance();
            cb
                .Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .InterceptTransparentProxy(options, typeof(IClientChannel));

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
                        var dict = client as IDictionary<int, int>;

                        Assert.NotNull(dict);

                        dict.Add(1, 2);

                        Assert.Equal(2, dict[1]);

                        dict.Clear();

                        Assert.Empty(dict);
                    }
                }
                finally
                {
                    host.Close();
                }
            }
        }

        [Fact]
        public void ServiceClientInterceptionIsPossibleWithSpecificInterceptors()
        {
            var options = new ProxyGenerationOptions { Selector = new MyInterceptorSelector() };

            // Build the service-side container
            var sb = new ContainerBuilder();
            sb.RegisterType<TestService>().As<ITestService>();

            // Build the client-side container with interception
            // around the client proxy. Issue 361 was that there
            // seemed to be trouble around getting this to work.
            var cb = new ContainerBuilder();
            cb.RegisterType<PrependInterceptor>();
            cb.RegisterType<AppendInterceptor>();
            cb.Register(c => CreateChannelFactory()).SingleInstance();
            cb
                .Register(c => c.Resolve<ChannelFactory<ITestService>>().CreateChannel())
                .InterceptTransparentProxy(options, typeof(IClientChannel))
                .InterceptedBy(typeof(PrependInterceptor), typeof(AppendInterceptor));

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

                        Assert.Equal("pre-service", client.DoWork());
                        Assert.Equal("work-post", client.DoOtherWork());
                    }
                }
                finally
                {
                    host.Close();
                }
            }
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

        public class AppendInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "DoOtherWork")
                {
                    invocation.ReturnValue += "-post";
                }
            }
        }

        public class InterceptOnlyOtherWork : IProxyGenerationHook
        {
            public void MethodsInspected()
            {
            }

            public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
            {
            }

            public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                return methodInfo.Name.Equals("DoOtherWork");
            }
        }

        public class PrependInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                invocation.ReturnValue = "pre-" + invocation.ReturnValue;
            }
        }

        public class TestService : ITestService
        {
            public string DoOtherWork()
            {
                return "work";
            }

            public string DoWork()
            {
                return "service";
            }
        }

        private class MyInterceptorSelector : IInterceptorSelector
        {
            public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
            {
                return method.Name == "DoWork"
                    ? interceptors.OfType<PrependInterceptor>().ToArray<IInterceptor>()
                    : interceptors.OfType<AppendInterceptor>().ToArray<IInterceptor>();
            }
        }
    }
}
