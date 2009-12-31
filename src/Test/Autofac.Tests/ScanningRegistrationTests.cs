using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Tests.Scenarios.ScannedAssembly;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Tests
{
    [TestFixture]
    public class ScanningRegistrationTests
    {
        [Test]
        public void WhenAssemblyIsScanned_TypesRegisteredByDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly);
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertSharing<AComponent>(InstanceSharing.None);
            c.AssertLifetime<AComponent, CurrentScopeLifetime>();
            c.AssertOwnership<AComponent>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Test]
        public void WhenFiltersApplied_NonMatchingTypesExcluded()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .Where(t => t.Name.StartsWith("A"))
                .Where(t => t.Name.StartsWith("AC"));
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertNotRegistered<BComponent>();
            c.AssertNotRegistered<A2Component>();
        }

        [Test]
        public void WhenServiceSpecifiedDirectly_AllMatchingTypesImplementIt()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
            c.AssertNotRegistered<AComponent>();
        }

        [Test]
        public void WhenServicesSpecifiedByFunction_EachTypeMappedIndependently()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(t => new NamedService(t.Name, typeof(object)));
            var c = cb.Build();

            c.AssertRegistered<object>("AComponent");
            c.AssertRegistered<object>("BComponent");
            c.AssertNotRegistered<AComponent>();
        }

        [Test]
        public void NameMappingRegistersNamedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .Named(t => t.Name, typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>("AComponent");
        }

        [Test]
        public void TypeMappingRegistersTypedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(t => typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
        }

        [Test]
        public void AsImplementedInterfaces_RegistersImplementedInterfaces()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertRegistered<IAService>();
            c.AssertRegistered<IBService>();
        }

        [Test]
        public void WhenFilterApplied_DefaultSelfRegistrationOmitted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertNotRegistered<AComponent>();
        }

        [Test]
        public void AsClosedTypesOf_NullTypeProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyTypes(typeof(ICommand<>).Assembly).
                AsClosedTypesOf(null));
        }

        [Test]
        public void AsClosedTypesOf_NonGenericTypeProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() => cb.RegisterAssemblyTypes(typeof(ICommand<>).Assembly).
                AsClosedTypesOf(typeof(SaveCommandData)));
        }

        [Test]
        public void AsClosedTypesOf_ClosedGenericTypeProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() => cb.RegisterAssemblyTypes(typeof(ICommand<>).Assembly).
                AsClosedTypesOf(typeof(ICommand<SaveCommandData>)));
        }

        [Test]
        public void AsClosedTypesOf_OpenGenericInterfaceTypeProvided_ClosingGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(ICommand<>).Assembly)
                .AsClosedTypesOf(typeof(ICommand<>));
            IContainer c = cb.Build();

            Assert.That(c.Resolve<ICommand<SaveCommandData>>(), Is.TypeOf<SaveCommand>());
            Assert.That(c.Resolve<ICommand<DeleteCommandData>>(), Is.TypeOf<DeleteCommand>());
        }

        [Test]
        public void AsClosedTypesOf_OpenGenericAbstractClassTypeProvided_ClosingGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(Message<>).Assembly)
                .AsClosedTypesOf(typeof(Message<>));
            IContainer c = cb.Build();

            Assert.That(c.Resolve<Message<string>>(), Is.TypeOf<StringMessage>());
        }
    }
}
