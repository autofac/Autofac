using System;
using System.ServiceModel;
using Autofac;
using Autofac.Extras.Multitenant.Wcf;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class AutofacHostFactoryFixture
    {
        [SetUp]
        public void SetUp()
        {
            AutofacHostFactory.Container = this.BuildContainer();
            AutofacHostFactory.HostConfigurationAction = null;
            AutofacHostFactory.ServiceImplementationDataProvider = null;
        }

        [TearDown]
        public void TearDown()
        {
            AutofacHostFactory.Container = null;
            AutofacHostFactory.HostConfigurationAction = null;
            AutofacHostFactory.ServiceImplementationDataProvider = null;
        }

        [Test(Description = "The default behavior when no different data provider is registered should be to create a service host given the contract type name.")]
        public void CreateServiceHost_CreatesHostUsingDefaultBehavior()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            Assert.IsNotNull(factory.CreateServiceHost(constructorString, baseAddresses), "No service host was created.");
        }

        [Test(Description = "Ensures you can't create a service host with an empty constructor string.")]
        public void CreateServiceHost_EmptyConstructorString()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = "";
            Assert.Throws<ArgumentException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null set of base addresses.")]
        public void CreateServiceHost_NullBaseAddresses()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = null;
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null constructor string.")]
        public void CreateServiceHost_NullConstructorString()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = null;
            Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Ensures you can't create a service host with a null application container.")]
        public void CreateServiceHost_NullContainer()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            AutofacHostFactory.Container = null;
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Checks the validation for the resolved service type to host. Also verifies that the data provider can be substituted.")]
        public void CreateServiceHost_ResolvedServiceTypeToHostIsNotClass()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            var provider = this.BuildTestDataProvider(constructorString, typeof(IServiceContract), l => new ServiceImplementation());
            AutofacHostFactory.ServiceImplementationDataProvider = provider;
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        [Test(Description = "Checks the validation for the resolved service type to host. Also verifies that the data provider can be substituted.")]
        public void CreateServiceHost_ResolvedServiceTypeToHostIsNull()
        {
            var factory = new TestHostFactory();
            Uri[] baseAddresses = new Uri[] { new Uri("http://localhost:12131/Foo.svc") };
            string constructorString = typeof(IServiceContract).AssemblyQualifiedName;
            var provider = this.BuildTestDataProvider(constructorString, null, l => new ServiceImplementation());
            AutofacHostFactory.ServiceImplementationDataProvider = provider;
            Assert.Throws<InvalidOperationException>(() => factory.CreateServiceHost(constructorString, baseAddresses));
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceImplementation>().As<IServiceContract>();
            return builder.Build();
        }

        private IServiceImplementationDataProvider BuildTestDataProvider(string ctorString, Type toHost, Func<ILifetimeScope, object> resolver)
        {
            return new TestServiceImplementationDataProvider()
            {
                Data = new ServiceImplementationData()
                {
                    ConstructorString = ctorString,
                    ServiceTypeToHost = toHost,
                    ImplementationResolver = resolver
                }
            };
        }

        private class TestHostFactory : AutofacHostFactory
        {
        }

        private class TestServiceImplementationDataProvider : IServiceImplementationDataProvider
        {
            public ServiceImplementationData Data { get; set; }

            public ServiceImplementationData GetServiceImplementationData(string constructorString)
            {
                return this.Data;
            }
        }

        [ServiceContract]
        public interface IServiceContract
        {
            [OperationContract]
            void Operation();
        }

        private class ServiceImplementation : IServiceContract
        {
            public void Operation()
            {
            }
        }
    }
}
