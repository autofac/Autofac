using Autofac;
using AutofacContrib.AggregateService;
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
            var container = builder.Build();

            var firstInstance = container.Resolve<IMyContext>();
            var secondInstance = container.Resolve<IMyContext>();

            Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
        }
    }
}