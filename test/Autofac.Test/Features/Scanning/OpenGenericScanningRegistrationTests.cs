// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;
using Autofac.Test.Scenarios.ScannedAssembly;
using Autofac.Test.Scenarios.ScannedAssembly.MetadataAttributeScanningScenario;
using Xunit;

namespace Autofac.Test.Features.Scanning
{
    public class OpenGenericScanningRegistrationTests
    {
        private static readonly Assembly ScenarioAssembly = typeof(AComponent).GetTypeInfo().Assembly;

        [Fact]
        public void WhenAssemblyIsScannedOpenGenericTypesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(AComponent).GetTypeInfo().Assembly);
            var c = cb.Build();

            Assert.NotNull(c.Resolve<RedoOpenGenericCommand<int>>());

            c.AssertRegistered<RedoOpenGenericCommand<int>>();
            c.AssertSharing<RedoOpenGenericCommand<int>>(InstanceSharing.None);
            c.AssertLifetime<RedoOpenGenericCommand<int>, CurrentScopeLifetime>();
            c.AssertOwnership<RedoOpenGenericCommand<int>>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void WhenOpenGenericTypesRegisteredAsSelfConcreteTypeIsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .AsSelf();
            var c = cb.Build();

            Assert.NotNull(c.Resolve<RedoOpenGenericCommand<int>>());

            c.AssertRegistered<RedoOpenGenericCommand<int>>();
        }

        [Fact]
        public void WhenFiltersAppliedNonMatchingOpenGenericTypesExcluded()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(AComponent).GetTypeInfo().Assembly)
                .Where(t => t.Name.StartsWith("Redo"));
            var c = cb.Build();

            Assert.NotNull(c.Resolve<RedoOpenGenericCommand<int>>());
            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<DeleteOpenGenericCommand<int>>());

            c.AssertRegistered<RedoOpenGenericCommand<int>>();
            c.AssertNotRegistered<DeleteOpenGenericCommand<int>>();
        }

        [Fact]
        public void WhenTypedServicesAreSpecifiedImplicitFilterApplied()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(A2Component).GetTypeInfo().Assembly)
                .As(typeof(IOpenGenericAService<>));
            var c = cb.Build();

            // Without the filter this line would throw anyway
            var a = c.Resolve<IEnumerable<IOpenGenericAService<int>>>();
            Assert.Single(a);
        }

        [Fact]
        public void WhenMappingToMultipleTypedServicesEachExposedAsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(A2Component).GetTypeInfo().Assembly)
                .As(t => t.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()));
            var c = cb.Build();

            Assert.NotNull(c.Resolve<IOpenGenericAService<int>>());
            Assert.NotNull(c.Resolve<IOpenGenericBService<int>>());
        }

        [Fact]
        public void WhenExceptionsProvideConfigurationComponentConfiguredAppropriately()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(ScenarioAssembly)
                .Except(typeof(RedoOpenGenericCommand<>), ac => ac.SingleInstance());
            var c = cb.Build();

            var a1 = c.Resolve<RedoOpenGenericCommand<int>>();
            var a2 = c.Resolve<RedoOpenGenericCommand<int>>();
            Assert.Same(a1, a2);
        }

        [Fact]
        public void AssignableToNullTypeProvidedThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(null));

            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(RedoOpenGenericCommand<>), (object)null));

            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(null, "serviceKey"));

            Assert.Throws<ArgumentNullException>(() => cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(null, t => t));
        }

        [Theory]
        [InlineData(typeof(ICloseCommand))]
        [InlineData(typeof(CloseCommand))]
        public void AssignableToClosedTypeProvidedNoneOpenGenericSourceRegistered(Type closedType)
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(closedType);
            var c = cb.Build();

            Assert.False(c.RegisteredAnyOpenGenericTypeFromScanningAssembly());
        }

        [Fact]
        public void ServiceIsNotAssignableToIsNotRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(RedoOpenGenericCommand<>));
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<DeleteOpenGenericCommand<int>>());
        }

        [Fact]
        public void AssignableToOpenGenericInterfaceTypeProvidedOpenGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(ICommand<>));
            var c = cb.Build();

            Assert.NotNull(c.Resolve<RedoOpenGenericCommand<int>>());
        }

        [Fact]
        public void AssignableToOpenGenericAbstractClassTypeProvidedOpenGenericTypesRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(CommandBase<>));
            var c = cb.Build();

            Assert.NotNull(c.Resolve<RedoOpenGenericCommand<int>>());
        }

        [Fact]
        public void AssignableToWithServiceKeyShouldAssignKeyToAllRegistrations()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(ICommand<>), "command");
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<DeleteOpenGenericCommand<int>>());
            Assert.NotNull(c.ResolveKeyed<RedoOpenGenericCommand<int>>("command"));
        }

        [Fact]
        public void AssignableToWithServiceKeyMappingShouldAssignKeyResultToEachRegistration()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AssignableTo(typeof(ICommand<>), t => t);
            var c = cb.Build();

            Assert.NotNull(c.ResolveKeyed<RedoOpenGenericCommand<int>>(typeof(RedoOpenGenericCommand<>)));
        }

        [Fact]
        public void AsImplementedInterfacesRegistersImplementedInterfaces()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            Assert.NotNull(c.Resolve<IOpenGenericAService<int>>());
        }

        [Fact]
        public void WhenFilterAppliedDefaultSelfRegistrationOmitted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<OpenGenericAComponent<int>>());
        }

        [Fact]
        public void AsSelfExposesConcreteTypeAsService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .AsImplementedInterfaces()
                .AsSelf();
            var c = cb.Build();

            Assert.NotNull(c.Resolve<OpenGenericAComponent<int>>());
        }

        [Fact]
        public void WhenMetadataMappingAppliedValuesCalculatedFromType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .WithMetadata(t => t.GetMethods().ToDictionary(m => m.Name, m => (object)m.ReturnType));

            var c = cb.Build();
            var s = c.Resolve<Meta<RedoOpenGenericCommand<int>>>();

            Assert.True(s.Metadata.ContainsKey("Execute"));
        }

        [Fact]
        public void WhenMetadataNotFoundThrowException()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                    .Where(t => t == typeof(OpenGenericScannedComponentWithName<>))
                    .WithMetadataFrom<ICloseCommand>();

            var ex = Assert.Throws<ArgumentException>(() => cb.Build());

            Assert.Equal(
                string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MetadataAttributeNotFound, typeof(ICloseCommand), typeof(OpenGenericScannedComponentWithName<>)),
                ex.Message);
        }

        [Fact]
        public void WhenMultipleMetadataAttributesSameTypeThrowException()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                    .Where(t => t == typeof(OpenGenericScannedComponentWithMultipleNames<>))
                    .WithMetadataFrom<IHaveName>();

            var ex = Assert.Throws<ArgumentException>(() => cb.Build());

            Assert.Equal(
                string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MultipleMetadataAttributesSameType, typeof(IHaveName), typeof(OpenGenericScannedComponentWithMultipleNames<>)),
                ex.Message);
        }

        [Fact]
        public void MetadataCanBeScannedFromAMatchingAttributeInterface()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .Where(t => t == typeof(OpenGenericScannedComponentWithName<>))
                .WithMetadataFrom<IHaveName>();

            var c = cb.Build();

            c.ComponentRegistry.TryGetRegistration(new TypedService(typeof(OpenGenericScannedComponentWithName<string>)), out IComponentRegistration r);

            r.Metadata.TryGetValue("Name", out object name);

            Assert.Equal("My Name", name);
        }

        [Fact]
        public void InNamespaceNullProvidedThrowException()
        {
            var cb = new ContainerBuilder();
            var ex = Assert.Throws<ArgumentNullException>(() =>
                cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly).InNamespace(ns: ""));

            Assert.Equal("ns", ex.ParamName);
        }

        [Fact]
        public void InNamespaceLimitsServicesToBeRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .InNamespace("Autofac.Test.Scenarios.ScannedAssembly.MetadataAttributeScanningScenario");

            var c = cb.Build();

            Assert.NotNull(c.Resolve<OpenGenericScannedComponentWithName<int>>());
            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<OpenGenericAComponent<int>>());
        }

        [Fact]
        public void InNamespaceOfLimitsServicesToBeRegistered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyOpenGenericTypes(typeof(ICommand<>).GetTypeInfo().Assembly)
                .InNamespaceOf<IHaveName>();

            var c = cb.Build();

            Assert.NotNull(c.Resolve<OpenGenericScannedComponentWithName<int>>());
            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<OpenGenericAComponent<int>>());
        }
    }
}
