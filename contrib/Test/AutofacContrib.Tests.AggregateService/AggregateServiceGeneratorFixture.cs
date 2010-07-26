using System;
using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceGeneratorFixture
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new Mock<IMyService>().Object);
            _container = builder.Build();
        }

        [Test]
        public void CreateInstance_WithGenericInterface_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance<IMyContext>(_container);

            Assert.That(instance, Is.InstanceOf<IMyContext>());
        }

        [Test]
        public void CreateInstance_WithInterfaceType_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance(typeof(IMyContext), _container);

            Assert.That(instance, Is.InstanceOf<IMyContext>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CreateInstance_ExpectsInterfaceTypeInstance()
        {
            AggregateServiceGenerator.CreateInstance(null, _container);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CreateInstance_ExpectsComponentInstance()
        {
            AggregateServiceGenerator.CreateInstance(typeof(IMyContext), null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CreateInstance_ExpectsInterfaceType()
        {
            AggregateServiceGenerator.CreateInstance<String>(_container);
        }
    }
}