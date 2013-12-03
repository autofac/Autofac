using System;
using System.ServiceModel;
using Autofac;
using Autofac.Extras.Multitenant.Wcf;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class MultitenantServiceImplementationDataProviderFixture
    {
        [Test(Description = "Attempts to get data using a constructor string that doesn't resolve to a type.")]
        public void GetServiceImplementationData_ConstructorStringDoesNotResolveToType()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            Assert.Throws<InvalidOperationException>(() => provider.GetServiceImplementationData("This is not a type name."));
        }

        [Test(Description = "Attempts to get data using a constructor string that doesn't resolve to an interface type.")]
        public void GetServiceImplementationData_ConstructorStringDoesNotResolveToInterface()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            Assert.Throws<InvalidOperationException>(() => provider.GetServiceImplementationData(typeof(Object).AssemblyQualifiedName));
        }

        [Test(Description = "Attempts to get data using a constructor string that doesn't resolve to a type marked as a service contract.")]
        public void GetServiceImplementationData_ConstructorStringDoesNotResolveToServiceContract()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            Assert.Throws<InvalidOperationException>(() => provider.GetServiceImplementationData(typeof(INotAServiceContract).AssemblyQualifiedName));
        }

        [Test(Description = "Verifies the data returned points to a proxy type.")]
        public void GetServiceImplementationData_DataReturnedBasedOnProxy()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            var data = provider.GetServiceImplementationData(typeof(IServiceContract).AssemblyQualifiedName);
            Assert.AreEqual(typeof(IServiceContract).AssemblyQualifiedName, data.ConstructorString, "The constructor string should be the interface name.");
            Assert.AreEqual(1, data.ServiceTypeToHost.FindInterfaces((f, o) => f == (Type)o, typeof(IServiceContract)).Length, "The service type to host should implement the service interface.");
        }

        [Test(Description = "Verifies the data returned can resolve a service implementation and wrap it in a proxy.")]
        public void GetServiceImplementationData_DataResolvesTypeThroughProxy()
        {
            var builder = new ContainerBuilder();
            var implementation = new ServiceImplementation();
            builder.RegisterInstance(implementation).As<IServiceContract>();
            var container = builder.Build();
            var provider = new MultitenantServiceImplementationDataProvider();
            var data = provider.GetServiceImplementationData(typeof(IServiceContract).AssemblyQualifiedName);
            var resolved = data.ImplementationResolver(container.BeginLifetimeScope());
            Assert.IsNotNull(resolved, "The proxy object resolved should not be null.");
            ((IServiceContract)resolved).MethodToProxy();
            Assert.IsTrue(implementation.ProxyMethodCalled, "The proxy object should proxy the methods on the implementation.");
        }

        [Test(Description = "Attempts to get data using an empty constructor string.")]
        public void GetServiceImplementationData_EmptyConstructorString()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            Assert.Throws<ArgumentException>(() => provider.GetServiceImplementationData(""));
        }

        [Test(Description = "Attempts to get data using a null constructor string.")]
        public void GetServiceImplementationData_NullConstructorString()
        {
            var provider = new MultitenantServiceImplementationDataProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetServiceImplementationData(null));
        }

        public interface INotAServiceContract
        {
            // Has to be public or Castle.DynamicProxy can't make a proxy.
        }

        [ServiceContract]
        public interface IServiceContract
        {
            // Has to be public or Castle.DynamicProxy can't make a proxy.
            void MethodToProxy();
        }

        private class ServiceImplementation : IServiceContract
        {
            public bool ProxyMethodCalled { get; set; }
            public void MethodToProxy()
            {
                this.ProxyMethodCalled = true;
            }
        }
    }
}
