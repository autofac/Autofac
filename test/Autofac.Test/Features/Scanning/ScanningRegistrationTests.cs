using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Features.Metadata;
using Autofac.Features.Scanning;
using Autofac.Test.Scenarios.ScannedAssembly;
using Autofac.Test.Scenarios.ScannedAssembly.MetadataAttributeScanningScenario;
using Xunit;

namespace Autofac.Test.Features.Scanning
{
    public class ScanningRegistrationTests
    {
        private static readonly Assembly ScenarioAssembly = typeof(AComponent).GetTypeInfo().Assembly;

        [Fact]
        public void WhenAssemblyIsScannedTypesRegisteredByDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly);
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertSharing<AComponent>(InstanceSharing.None);
            c.AssertLifetime<AComponent, CurrentScopeLifetime>();
            c.AssertOwnership<AComponent>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void WhenTypesRegisteredAsSelfConcreteTypeIsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .AsSelf();
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
        }

        [Fact]
        public void WhenNameAndMetadataMappingAppliedValueCalculatedFromType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .WithMetadata("Name", t => t.Name);

            var c = cb.Build();

            var a = c.Resolve<Meta<AComponent>>();

            Assert.Equal(typeof(AComponent).Name, a.Metadata["Name"]);
        }

        [Fact]
        public void WhenMetadataMappingAppliedValuesCalculatedFromType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(SaveCommand).GetTypeInfo().Assembly)
                .Where(t => t != typeof(UndoRedoCommand))
                .WithMetadata(t => t.GetMethods().ToDictionary(m => m.Name, m => (object)m.ReturnType));

            var c = cb.Build();
            var s = c.Resolve<Meta<SaveCommand>>();

            Assert.True(s.Metadata.ContainsKey("Execute"));
        }

        [Fact]
        public void WhenFiltersAppliedNonMatchingTypesExcluded()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .Where(t => t.Name.StartsWith("A"))
                .Where(t => t.Name.StartsWith("AC"));
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertNotRegistered<BComponent>();
            c.AssertNotRegistered<A2Component>();
        }

        [Fact]
        public void WhenServiceSpecifiedDirectlyAllMatchingTypesImplementIt()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .As(typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
            c.AssertNotRegistered<AComponent>();
        }

        [Fact]
        public void WhenServicesSpecifiedByFunctionEachTypeMappedIndependently()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .As(t => new KeyedService(t.Name, typeof(object)));
            var c = cb.Build();

            c.AssertRegistered<object>("AComponent");
            c.AssertRegistered<object>("BComponent");
            c.AssertNotRegistered<AComponent>();
        }

        [Fact]
        public void NameMappingRegistersKeyedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .Named(t => t.Name, typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>("AComponent");
        }

        [Fact]
        public void NameMappingRegistersKeyedServicesWithGenericSyntax()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .Named<object>(t => t.Name);
            var c = cb.Build();

            c.AssertRegistered<object>("AComponent");
        }

        [Fact]
        public void TypeMappingRegistersTypedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .As(t => typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
        }

        [Fact]
        public void AsImplementedInterfacesRegistersImplementedInterfaces()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertRegistered<IAService>();
            c.AssertRegistered<IBService>();
        }

        [Fact]
        public void WhenFilterAppliedDefaultSelfRegistrationOmitted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertNotRegistered<AComponent>();
        }

        [Fact]
        public void AsClosedTypesOfNullTypeProvidedThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyTypes(typeof(ICommand<>).GetTypeInfo().Assembly).
                AsClosedTypesOf(null));
        }

        [Fact]
        public void AsClosedTypesOfOpenGenericInterfaceTypeProvidedClosingGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(ICommand<>));
            var c = cb.Build();

            Assert.IsType<SaveCommand>(c.Resolve<ICommand<SaveCommandData>>());
            Assert.IsType<DeleteCommand>(c.Resolve<ICommand<DeleteCommandData>>());
        }

        [Fact]
        public void AsClosedTypesOfOpenGenericAbstractClassTypeProvidedClosingGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(Message<>).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(Message<>));
            var c = cb.Build();

            Assert.IsType<StringMessage>(c.Resolve<Message<string>>());
        }

        [Fact]
        public void AsClosedTypesOfClosingInterfaceTypeRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(ICloseCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(ICommand<>));
            var c = cb.Build();

            Assert.IsType<CloseCommand>(c.Resolve<ICloseCommand>());
        }

        [Fact]
        public void AsSelfExposesConcreteTypeAsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(A2Component).GetTypeInfo().Assembly)
                .AsImplementedInterfaces()
                .AsSelf();
            var c = cb.Build();

            Assert.True(c.IsRegistered<A2Component>());
        }

        [Fact]
        public void AsClosedTypesOfMultipleServicesPerClassExposesAllServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(UndoRedoCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(ICommand<>));
            var c = cb.Build();

            var r = c.RegistrationFor<ICommand<UndoCommandData>>();
            Assert.True(r.Services.Contains(new TypedService(typeof(ICommand<RedoCommandData>))));
        }

        [Fact]
        public void AsClosedTypesOfWithServiceKeyShouldAssignKeyToAllRegistrations()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(ICommand<>), "command");
            var c = cb.Build();

            Assert.IsType<SaveCommand>(c.ResolveKeyed<ICommand<SaveCommandData>>("command"));
            Assert.IsType<DeleteCommand>(c.ResolveKeyed<ICommand<DeleteCommandData>>("command"));
        }

        [Fact]
        public void AsClosedTypesOfWithServiceKeyMappingShouldAssignKeyResultToEachRegistration()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(ICommand<>), t => t);
            var c = cb.Build();

            Assert.IsType<SaveCommand>(c.ResolveKeyed<ICommand<SaveCommandData>>(typeof(SaveCommand)));
            Assert.IsType<DeleteCommand>(c.ResolveKeyed<ICommand<DeleteCommandData>>(typeof(DeleteCommand)));
        }

        [Fact]
        public void DoesNotIncludeDelegateTypesThusNotOverridingGeneratedFactories()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(HasNestedFactoryDelegate).GetTypeInfo().Assembly);
            var c = cb.Build();
            c.Resolve<HasNestedFactoryDelegate.Factory>();
        }

        [Fact]
        public void WhenScannedTypesAreRegisteredOnRegisteredHandlersAreCalled()
        {
            var onRegisteredCalled = false;

            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .OnRegistered(e => onRegisteredCalled = true);
            cb.Build();

            Assert.True(onRegisteredCalled);
        }

        [Fact]
        public void WhenTypedServicesAreSpecifiedImplicitFilterApplied()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(A2Component).GetTypeInfo().Assembly)
                .As<IAService>();
            var c = cb.Build();

            // Without the filter this line would throw anyway
            var a = c.Resolve<IEnumerable<IAService>>();
            Assert.Equal(1, a.Count());
        }

        [Fact]
        public void WhenExceptionsProvideConfigurationComponentConfiguredAppropriately()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .Except<AComponent>(ac => ac.SingleInstance());
            var c = cb.Build();
            var a1 = c.Resolve<AComponent>();
            var a2 = c.Resolve<AComponent>();
            Assert.Same(a1, a2);
        }

        [Fact]
        public void SingleRegistrationCanBeRegisteredAsSelf()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<A2Component>()
                .As<IAService>()
                .AsSelf();

            var c = cb.Build();
            c.AssertRegistered<A2Component>();
            c.AssertRegistered<IAService>();
        }

        [Fact]
        public void SingleRegistrationCanBeRegisteredAsImplementedInterfaces()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<A2Component>()
                .AsImplementedInterfaces();

            var c = cb.Build();
            c.AssertRegistered<IAService>();
            c.AssertRegistered<IBService>();
        }

        [Fact]
        public void WhenTransformingTypesToServicesNonAssignableServicesAreExcluded()
        {
            var cb = new ContainerBuilder();

            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .As(t => new KeyedService("foo", typeof(ScanningRegistrationTests)));

            var c = cb.Build();

            Assert.False(c.IsRegisteredWithKey<ScanningRegistrationTests>("foo"));
        }

        [Fact]
        public void WhenTransformingTypesToServicesComponentsWithNoServicesAreExcluded()
        {
            var cb = new ContainerBuilder();

            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .As(t => new KeyedService("foo", typeof(ScanningRegistrationTests)));

            var c = cb.Build();

            Assert.False(c.ComponentRegistry.Registrations.Any(r =>
                r.Activator.LimitType == typeof(AComponent)));
        }

        [Fact]
        public void WhenMappingToMultipleTypedServicesEachExposedAsService()
        {
            var c = RegisterScenarioAssembly(a => a.As(t => t.GetInterfaces()));
            var cd1 = c.ComponentRegistry.Registrations.Single(r => r.Activator.LimitType == typeof(A2Component));
            Assert.True(cd1.Services.Contains(new TypedService(typeof(IAService))));
            Assert.True(cd1.Services.Contains(new TypedService(typeof(IBService))));
        }

        [Fact]
        public void ByDefaultIDisposableIsNotAServiceInterface()
        {
            var c = RegisterScenarioAssembly(a => a.AsImplementedInterfaces());
            Assert.False(c.IsRegistered<IDisposable>());
        }

        [Fact]
        public void WhenDerivingKeysDynamically_TheCorrectOverloadIsChosen()
        {
            const string key = "akey";
            var c = RegisterScenarioAssembly(a => a.Keyed<IAService>(t => key));
            Assert.True(c.IsRegisteredWithKey<IAService>(key));
        }

        [Fact]
        public void PreserveExistingDefaults()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<AnExistingComponent>().As<IAService>();
            cb.RegisterAssemblyTypes(ScenarioAssembly)
                .As<IAService>()
                .PreserveExistingDefaults();

            var c = cb.Build();

            c.AssertComponentRegistrationOrder<IAService, AnExistingComponent, A2Component>();
        }

        public IContainer RegisterScenarioAssembly(Action<IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>> configuration = null)
        {
            var cb = new ContainerBuilder();
            var config = cb.RegisterAssemblyTypes(ScenarioAssembly);
            if (configuration != null)
                configuration(config);
            return cb.Build();
        }

        /// <summary>
        /// Test class used in the <see cref="ScanningRegistrationTests.PreserveExistingDefaults"/> test case.
        /// </summary>
        private class AnExistingComponent : IAService
        {
        }

        [Fact]
        public void MetadataCanBeScannedFromAMatchingAttributeInterface()
        {
            var c = RegisterScenarioAssembly(a => a
                .Where(t => t == typeof(ScannedComponentWithName))
                .WithMetadataFrom<IHaveName>());

            IComponentRegistration r;
            c.ComponentRegistry.TryGetRegistration(new TypedService(typeof(ScannedComponentWithName)), out r);

            object name;
            r.Metadata.TryGetValue("Name", out name);

            Assert.Equal("My Name", name);
        }

        [Fact]
        public void ScanningKeyedRegistrationsFilterByAssignabilityBeforeMappingKey()
        {
            const string k = "key";
            var c = RegisterScenarioAssembly(a => a.Keyed<IAService>(t =>
            {
                Assert.True(typeof(IAService).IsAssignableFrom(t));
                return k;
            }));
            Assert.True(c.IsRegisteredWithKey<IAService>(k));
        }

        [Fact]
        public void DeferredEnumerableHelperClassDoesNotGetRegistered()
        {
            var c = RegisterScenarioAssembly(a => a.AsImplementedInterfaces());

            var implementations = c.Resolve<IEnumerable<IHaveDeferredEnumerable>>();

            Assert.Equal(1, implementations.Count());
        }
    }
}
