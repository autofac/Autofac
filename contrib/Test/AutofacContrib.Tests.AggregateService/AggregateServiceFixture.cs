using System;
using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceFixture
    {
        private IContainer _container;
        private ISomeDependency _someDependencyMock;
        private IMyContext _aggregateService;

        [SetUp]
        public void Setup()
        {
            _someDependencyMock = new Mock<ISomeDependency>().Object;

            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            builder.RegisterType<MyServiceImpl>()
                .As<IMyService>()
                .InstancePerDependency();
            builder.RegisterInstance(_someDependencyMock);
            _container = builder.Build();

            _aggregateService = _container.Resolve<IMyContext>();
        }

        [Test]
        public void Property_ResolvesService()
        {
            Assert.That(_aggregateService.MyService,
                Is.Not.Null & Is.InstanceOf<IMyService>());
        }

        [Test]
        public void Property_Getter_AlwaysReturnSameInstance()
        {
            var firstInstance = _aggregateService.MyService;
            var secondInstance = _aggregateService.MyService;

            Assert.That(firstInstance, Is.SameAs(secondInstance));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Property_Setter_Throws()
        {
            _aggregateService.PropertyWithSetter = null;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Method_WithVoid_Throws()
        {
            _aggregateService.MethodWithoutReturnValue();
        }

        [Test]
        public void Method_ResolvesService()
        {
            Assert.That(_aggregateService.GetMyService(),
                Is.Not.Null & Is.InstanceOf<IMyService>());
        }

        [Test]
        public void Method_WithParameter_PassesParameterToService()
        {
            var myService = _aggregateService.GetMyService(10);

            Assert.That(myService.SomeIntValue, Is.EqualTo(10));
        }

        [Test]
        public void Method_WithParameters_PassesParametersToService()
        {
            var someDate = DateTime.Now;
            var myService = _aggregateService.GetMyService(someDate, 20);

            Assert.That(myService.SomeDateValue, Is.EqualTo(someDate));
            Assert.That(myService.SomeIntValue, Is.EqualTo(20));
        }

        [Test]
        public void Method_WithNullParameters_PassesParametersToService()
        {
            var myService = _aggregateService.GetMyService(null);

            Assert.That(myService.SomeStringValue, Is.Null);
        }

        [Test]
        public void Method_WithParameter_PassesParameterAndOtherDependenciesToService()
        {
            var myService = _aggregateService.GetMyService("text");

            Assert.That(myService.SomeStringValue, Is.EqualTo("text"));
            Assert.That(myService.SomeDependency, Is.EqualTo(_someDependencyMock));
        }
    }
}