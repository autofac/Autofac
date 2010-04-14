using System;
using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceFixture
    {
        private IContainer _container;
        private ISomeDependency _someDependencyMock;

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            builder.RegisterType<MyServiceImpl>().As<IMyService>();
            _someDependencyMock = new Mock<ISomeDependency>().Object;
            builder.RegisterInstance(_someDependencyMock);
            _container = builder.Build();
        }

        [Test]
        public void Property_ResolvesService()
        {
            var dep = _container.Resolve<IMyContext>();

            Assert.That(dep.MyService, Is.Not.Null & Is.InstanceOfType(typeof(IMyService)));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Property_WithSetter_Throws()
        {
            var dep = _container.Resolve<IMyContext>();
            dep.PropertyWithSetter = null;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Method_WithVoid_Throws()
        {
            var dep = _container.Resolve<IMyContext>();
            dep.MethodWithoutReturnValue();
        }

        [Test]
        public void Method_ResolvesService()
        {
            var dep = _container.Resolve<IMyContext>();
            
            Assert.That(dep.GetMyService(), Is.Not.Null & Is.InstanceOfType(typeof(IMyService)));
        }

        [Test]
        public void Method_WithParameter_PassesParameterToService()
        {
            var dep = _container.Resolve<IMyContext>();
            var myService = dep.GetMyService(10);

            Assert.That(myService.SomeIntValue, Is.EqualTo(10));
        }

        [Test]
        public void Method_WithParameters_PassesParametersToService()
        {
            var someDate = DateTime.Now;
            var dep = _container.Resolve<IMyContext>();
            var myService = dep.GetMyService(someDate, 20);

            Assert.That(myService.SomeDateValue, Is.EqualTo(someDate));
            Assert.That(myService.SomeIntValue, Is.EqualTo(20));
        }

        [Test]
        public void Method_WithNullParameters_PassesParametersToService()
        {
            var dep = _container.Resolve<IMyContext>();
            var myService = dep.GetMyService((string)null);

            Assert.That(myService.SomeStringValue, Is.Null);
        }

        [Test]
        public void Method_WithParameter_PassesParameterAndOtherDependenciesToService()
        {
            var dep = _container.Resolve<IMyContext>();
            var myService = dep.GetMyService("text");

            Assert.That(myService.SomeStringValue, Is.EqualTo("text"));
            Assert.That(myService.SomeDependency, Is.EqualTo(_someDependencyMock));
        }

        
    }
}