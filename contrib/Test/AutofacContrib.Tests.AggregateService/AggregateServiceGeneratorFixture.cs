using System;
using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

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

            Assert.That(instance, Is.InstanceOfType(typeof(IMyContext)));
        }

        [Test]
        public void CreateInstance_WithInterfaceType_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance(typeof(IMyContext), _container);

            Assert.That(instance, Is.InstanceOfType(typeof(IMyContext)));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CreateInstance_ExpectsInterfaceTypeInstance()
        {
            AggregateServiceGenerator.CreateInstance(null, _container);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CreateInstance_ExpectsInterfaceType()
        {
            AggregateServiceGenerator.CreateInstance<String>(_container);
        }
    }
}