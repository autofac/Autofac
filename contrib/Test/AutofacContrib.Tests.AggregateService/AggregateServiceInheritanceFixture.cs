using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceInheritanceFixture
    {
        private IContainer _container;
        private ISubService _aggregateService;
        private ISomeDependency _someDependencyMock;
        private ISomeOtherDependency _someOtherDependencyMock;

        [SetUp]
        public void Setup()
        {
            _someDependencyMock = new Mock<ISomeDependency>().Object;
            _someOtherDependencyMock = new Mock<ISomeOtherDependency>().Object;

            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<ISubService>();
            builder.RegisterInstance(_someDependencyMock);
            builder.RegisterInstance(_someOtherDependencyMock);
            _container = builder.Build();

            _aggregateService = _container.Resolve<ISubService>();
        }

        [Test]
        public void Resolve_PropertyOnSuperType()
        {
            Assert.That(_aggregateService.SomeDependency, Is.EqualTo(_someDependencyMock));
        }

        [Test]
        public void Resolve_PropertyOnSubType()
        {
            Assert.That(_aggregateService.SomeOtherDependency, Is.EqualTo(_someOtherDependencyMock));
        }
    }
}
