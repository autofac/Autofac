using System;
using System.ServiceModel;
using Autofac.Integration.Wcf;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Wcf
{
    [TestFixture]
    public abstract class AutofacHostFactoryFixtureBase<T> where T : AutofacHostFactory, new()
    {
        readonly Uri[] _dummyEndpoints = new[] { new Uri("http://localhost") };

        [Test]
        public void NullConstructorStringThrowsException()
        {
            var factory = new T();
            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceHost(null, _dummyEndpoints));
            Assert.That(exception.ParamName, Is.EqualTo("constructorString"));
        }

        [Test]
        public void EmptyConstructorStringThrowsException()
        {
            var factory = new T();
            var exception = Assert.Throws<ArgumentException>(() => factory.CreateServiceHost(string.Empty, _dummyEndpoints));
            Assert.That(exception.ParamName, Is.EqualTo("constructorString"));
        }

        [Test]
        public void HostsKeyedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named<object>("service");
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost("service", _dummyEndpoints);
                Assert.IsNotNull(host);
            });
        }

        [Test]
        public void HostsTypedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                Assert.IsNotNull(host);
            });
        }

        [Test]
        public void HostsTypedServicesAsServices()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => "Test").As<object>();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                Assert.IsNotNull(host);
                Assert.AreEqual(typeof(string), host.Description.ServiceType);
            });
        }

        [Test]
        public void NonSingletonServiceMustNotBeRegisteredAsSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().SingleInstance();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var exception = Assert.Throws<InvalidOperationException>(
                    () => factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints));
                string expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustNotBeSingleInstance, typeof(object).FullName);
                Assert.That(exception.Message, Is.EqualTo(expectedMessage));
            });
        }

        [Test]
        public void HostsSingletonServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSingletonService>().SingleInstance();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var host = factory.CreateServiceHost(typeof(TestSingletonService).AssemblyQualifiedName, _dummyEndpoints);
                Assert.IsNotNull(host);
                Assert.AreEqual(typeof(TestSingletonService), host.Description.ServiceType);
            });
        }

        [Test]
        public void SingletonServiceMustBeRegisteredAsSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSingletonService>().InstancePerDependency();
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                var exception = Assert.Throws<InvalidOperationException>(
                    () => factory.CreateServiceHost(typeof(TestSingletonService).AssemblyQualifiedName, _dummyEndpoints));
                string expectedMessage = string.Format(AutofacHostFactoryResources.ServiceMustBeSingleInstance, typeof(TestSingletonService).FullName);
                Assert.That(exception.Message, Is.EqualTo(expectedMessage));
            });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DetectsUnknownImplementationTypes()
        {
            var builder = new ContainerBuilder();
            builder.Register<ITestService>(c => new TestService()).Named<object>("service");
            TestWithHostedContainer(builder.Build(), () =>
            {
                var factory = new T();
                factory.CreateServiceHost("service", _dummyEndpoints);
            });
        }

        [Test]
        public void ExecutesHostConfigurationActionWhenSet()
        {
            try
            {
                ServiceHostBase hostParameter = null;
                ServiceHostBase actualHost = null;
                bool actionCalled = false;

                AutofacHostFactory.HostConfigurationAction = host =>
                {
                    hostParameter = host;
                    actionCalled = true;
                };

                var builder = new ContainerBuilder();
                builder.RegisterType<object>();
                TestWithHostedContainer(builder.Build(), () =>
                {
                    var factory = new T();
                    actualHost = factory.CreateServiceHost(typeof(object).FullName, _dummyEndpoints);
                    Assert.IsNotNull(actualHost);
                });

                Assert.AreSame(hostParameter, actualHost);
                Assert.IsTrue(actionCalled);
            }
            finally
            {
                AutofacHostFactory.HostConfigurationAction = null;
            }
        }

        static void TestWithHostedContainer(IContainer container, Action test)
        {
            AutofacHostFactory.Container = container;
            try
            {
                test();
            }
            finally
            {
                AutofacHostFactory.Container = null;
            }
        }
    }
}