using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using NUnit.Framework;
using Autofac.Tests.Scenarios.ScannedAssembly;
using Autofac.Core;
using Autofac.Core.Lifetime;
using System.Reflection;

namespace Autofac.Tests.Features.Scanning
{
    [TestFixture]
    public class ScanningRegistrationTests
    {
        static readonly Assembly ScenarioAssembly = typeof(AComponent).Assembly;

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
        public void WhenTypesRegisteredAsSelf_ConcreteTypeIsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .AsSelf();
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
        }

        [Test]
        public void WhenNameAndMetadataMappingApplied_ValueCalculatedFromType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .WithMetadata("Name", t => t.Name);

            var c = cb.Build();

            var a = c.Resolve<Meta<AComponent>>();

            Assert.AreEqual(typeof(AComponent).Name, a.Metadata["Name"]);
        }

        [Test]
        public void WhenMetadataMappingApplied_ValuesCalculatedFromType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(SaveCommand).Assembly)
                .Where(t => t != typeof(UndoRedoCommand))
                .WithMetadata(t => t.GetMethods().ToDictionary(m => m.Name, m => (object)m.ReturnType));

            var c = cb.Build();
            var s = c.Resolve<Meta<SaveCommand>>();

            Assert.IsTrue(s.Metadata.ContainsKey("Execute"));
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
            var c = cb.Build();

            Assert.That(c.Resolve<ICommand<SaveCommandData>>(), Is.TypeOf<SaveCommand>());
            Assert.That(c.Resolve<ICommand<DeleteCommandData>>(), Is.TypeOf<DeleteCommand>());
        }

        [Test]
        public void AsClosedTypesOf_OpenGenericAbstractClassTypeProvided_ClosingGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(Message<>).Assembly)
                .AsClosedTypesOf(typeof(Message<>));
            var c = cb.Build();

            Assert.That(c.Resolve<Message<string>>(), Is.TypeOf<StringMessage>());
        }

        [Test]
        public void AsSelf_ExposesConcreteTypeAsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(A2Component).Assembly)
                .AsImplementedInterfaces()
                .AsSelf();
            var c = cb.Build();

            Assert.That(c.IsRegistered<A2Component>());
        }

        [Test]
        public void AsClosedTypesOf_MultipleServicesPerClass_ExposesAllServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(UndoRedoCommand).Assembly)
                .AsClosedTypesOf(typeof(ICommand<>));
            var c = cb.Build();

            var r = c.RegistrationFor<ICommand<UndoCommandData>>();
            Assert.That(r.Services.Contains(new TypedService(typeof(ICommand<RedoCommandData>))));
        }

        [Test]
        public void DoesNotIncludeDelegateTypes_ThusNotOverridingGeneratedFactories()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(HasNestedFactoryDelegate).Assembly);
            var c = cb.Build();
            c.Resolve<HasNestedFactoryDelegate.Factory>();
        }

        [Test]
        public void WhenScannedTypesAreRegistered_OnRegisteredHandlersAreCalled()
        {
            var onRegisteredCalled = false;

            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .OnRegistered(e => onRegisteredCalled = true);
            cb.Build();

            Assert.That(onRegisteredCalled);
        }

        [Test]
        public void WhenTypedServicesAreSpecified_ImplicitFilterApplied()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(A2Component).Assembly)
                .As<IAService>();
            var c = cb.Build();
            // Without the filter this line would throw anyway
            var a = c.Resolve<IEnumerable<IAService>>();
            Assert.AreEqual(1, a.Count());
        }

        [Test]
        public void WhenExceptionsProvideConfiguration_ComponentConfiguredAppropriately()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .Except<AComponent>(ac => ac.SingleInstance());
            var c = cb.Build();
            var a1 = c.Resolve<AComponent>();
            var a2 = c.Resolve<AComponent>();
            Assert.AreSame(a1, a2);
        }

        [Test, Ignore("These cases might be better off failing fast.")]
        public void WhenMultipleServicesAreSpecified_NonMatchingFunctionOfType_ExcludesType()
        {
            var cb = new ContainerBuilder();

            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .AsSelf()
                .As(t => new KeyedService("foo", typeof(ScanningRegistrationTests)));

            cb.Build().AssertNotRegistered<AComponent>();
        }

        //[Test]
        //public void SingleRegistration_CanBeRegistered_AsSelf()
        //{
        //    var cb = new ContainerBuilder();

        //    cb.RegisterType<A2Component>()
        //        .As<IAService>()
        //        .AsSelf();

        //    var c = cb.Build();
        //    c.AssertRegistered<A2Component>();
        //    c.AssertRegistered<IAService>();
        //}

        //[Test]
        //public void SingleRegistration_CanBeRegistered_AsImplementedInterfaces()
        //{
        //    var cb = new ContainerBuilder();

        //    cb.RegisterType<A2Component>()
        //        .AsImplementedInterfaces();

        //    var c = cb.Build();
        //    c.AssertRegistered<IAService>();
        //    c.AssertRegistered<IBService>();
        //}
    }
}
