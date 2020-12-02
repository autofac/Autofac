// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Scanning;
using Autofac.Test.Scenarios.ScannedAssembly;
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
    }
}
