using System;
using Autofac;
using AutofacContrib.AggregateService;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class ContainerBuilderExtensionsFixture
    {
        [Test]
        public void RegisterAggregateService_WithGeneric_RegistersServiceInterface()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            var container = builder.Build();
            
            Assert.That(container.IsRegistered<IMyContext>());
        }

        [Test]
        public void RegisterAggregateService_WithType_RegistersServiceInterface()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IMyContext));
            var container = builder.Build();
            
            Assert.That(container.IsRegistered<IMyContext>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAggregateService_WithNullInterfaceType_ThrowsArgumentNullException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void RegisterAggregateService_WithNonInterfaceType_ThrowsArgumentException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(MyServiceImpl));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void RegisterAggregateService_WithGenericNonInterfaceType_ThrowsArgumentException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<MyServiceImpl>();
        }

        [Test]
        public void RegisterAggregateService_DifferentLifeTimeScopes_YieldsDifferentInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IMyContext));
            builder.RegisterType<MyServiceImpl>()
                .As<IMyService>()
                .InstancePerLifetimeScope();
            var container = builder.Build();

            var rootScope = container.Resolve<IMyContext>();
            var subScope = container.BeginLifetimeScope().Resolve<IMyContext>();

            Assert.That(rootScope.MyService, Is.Not.SameAs(subScope.MyService));
        }

        [Test]
        public void RegisterAggregateService_IsPerDependencyScoped()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            builder.RegisterInstance(new Mock<IMyService>().Object);
            var container = builder.Build();

            var firstInstance = container.Resolve<IMyContext>();
            var secondInstance = container.Resolve<IMyContext>();

            Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
        }
    }
}