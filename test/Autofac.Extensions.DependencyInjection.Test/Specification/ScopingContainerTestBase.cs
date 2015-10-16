// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public abstract class ScopingContainerTestBase : AllContainerTestsBase
    {
        [Fact]
        public void LastServiceReplacesPreviousServices()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeMultipleService>();

            Assert.Equal("FakeTwoMultipleServiceAnotherMethod", service.SimpleMethod());
        }

        [Fact]
        public void SingletonServiceCanBeResolved()
        {
            var container = CreateContainer();

            var service1 = container.GetService<IFakeSingletonService>();
            var service2 = container.GetService<IFakeSingletonService>();

            Assert.NotNull(service1);
            Assert.Equal(service1, service2);
        }

        [Fact]
        public void ScopedServiceCanBeResolved()
        {
            var container = CreateContainer();

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var containerScopedService = container.GetService<IFakeScopedService>();
                var scopedService1 = scope.ServiceProvider.GetService<IFakeScopedService>();
                Thread.Sleep(200);
                var scopedService2 = scope.ServiceProvider.GetService<IFakeScopedService>();

                Assert.NotEqual(containerScopedService, scopedService1);
                Assert.Equal(scopedService1, scopedService2);
            }
        }

        [Fact]
        public void NestedScopedServiceCanBeResolved()
        {
            var container = CreateContainer();

            var outerScopeFactory = container.GetService<IServiceScopeFactory>();
            using (var outerScope = outerScopeFactory.CreateScope())
            {
                var innerScopeFactory = outerScope.ServiceProvider.GetService<IServiceScopeFactory>();
                using (var innerScope = innerScopeFactory.CreateScope())
                {
                    var outerScopedService = outerScope.ServiceProvider.GetService<IFakeScopedService>();
                    var innerScopedService = innerScope.ServiceProvider.GetService<IFakeScopedService>();

                    Assert.NotEqual(outerScopedService, innerScopedService);
                }
            }
        }

        [Fact]
        public void DisposingScopeDisposesService()
        {
            var container = CreateContainer();
            FakeService disposableService;
            FakeService transient1;
            FakeService transient2;
            FakeService transient3;
            FakeService singleton;

            transient3 = (FakeService)container.GetService<IFakeService>();

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                disposableService = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();
                transient1 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                transient2 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                singleton = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();

                Assert.False(disposableService.Disposed);
                Assert.False(transient1.Disposed);
                Assert.False(transient2.Disposed);
                Assert.False(singleton.Disposed);
            }

            Assert.True(disposableService.Disposed);
            Assert.True(transient1.Disposed);
            Assert.True(transient2.Disposed);
            Assert.False(singleton.Disposed);

            var disposableContainer = container as IDisposable;
            if (disposableContainer != null)
            {
                disposableContainer.Dispose();
                Assert.True(singleton.Disposed);
                Assert.True(transient3.Disposed);
            }
        }

        [Fact]
        public void SelfResolveThenDispose()
        {
            var container = CreateContainer();

            var serviceProvider = container.GetServices<IServiceProvider>();

            (container as IDisposable)?.Dispose();
        }

        [Fact]
        public void SafelyDisposeNestedProviderReferences()
        {
            var container = CreateContainer();

            var nester = container.GetService<ClassWithNestedReferencesToProvider>();

            nester.Dispose();
        }

        [Fact]
        public void SingletonServicesComeFromRootContainer()
        {
            var container = CreateContainer();
            FakeService disposableService1;
            FakeService disposableService2;

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                disposableService1 = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();

                Assert.False(disposableService1.Disposed);
            }
            Assert.False(disposableService1.Disposed);

            using (var scope = scopeFactory.CreateScope())
            {
                disposableService2 = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();

                Assert.False(disposableService2.Disposed);
            }
            Assert.False(disposableService2.Disposed);

            Assert.Same(disposableService1, disposableService2);
        }

        [Fact]
        public void NestedScopedServiceCanBeResolvedWithNoFallbackProvider()
        {
            var container = CreateContainer();

            var outerScopeFactory = container.GetService<IServiceScopeFactory>();
            using (var outerScope = outerScopeFactory.CreateScope())
            {
                var innerScopeFactory = outerScope.ServiceProvider.GetService<IServiceScopeFactory>();
                using (var innerScope = innerScopeFactory.CreateScope())
                {
                    var outerScopedService = outerScope.ServiceProvider.GetService<IFakeScopedService>();
                    var innerScopedService = innerScope.ServiceProvider.GetService<IFakeScopedService>();

                    Assert.NotEqual(outerScopedService, innerScopedService);
                }
            }
        }

        [Fact]
        public void NestedScopedServiceCanBeResolvedWithNonScopingFallbackProvider()
        {
            var container = CreateContainer();

            var outerScopeFactory = container.GetService<IServiceScopeFactory>();
            using (var outerScope = outerScopeFactory.CreateScope())
            {
                var innerScopeFactory = outerScope.ServiceProvider.GetService<IServiceScopeFactory>();
                using (var innerScope = innerScopeFactory.CreateScope())
                {
                    var outerScopedService = outerScope.ServiceProvider.GetService<IFakeScopedService>();
                    var innerScopedService = innerScope.ServiceProvider.GetService<IFakeScopedService>();

                    Assert.NotEqual(outerScopedService, innerScopedService);
                }
            }
        }

        [Fact]
        public void OpenGenericServicesCanBeResolved()
        {
            var container = CreateContainer();

            var genericService = container.GetService<IFakeOpenGenericService<IFakeSingletonService>>();
            var singletonService = container.GetService<IFakeSingletonService>();

            Assert.Equal(singletonService, genericService.SimpleMethod());
        }

        [Fact]
        public void ClosedServicesPreferredOverOpenGenericServices()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeOpenGenericService<string>>();

            Assert.Equal("FakeServiceSimpleMethod", service.SimpleMethod());
        }

        [Fact]
        public void AttemptingToResolveNonexistentServiceReturnsNull()
        {
            var container = CreateContainer();

            Assert.Null(container.GetService<INonexistentService>());
        }

        [Fact]
        public void AttemptingToResolveNonexistentServiceIndirectlyThrows()
        {
            var container = CreateContainer();

            Assert.ThrowsAny<Exception>(() => container.GetService<IDependOnNonexistentService>());
        }

        [Fact]
        public void NonexistentServiceCanBeIEnumerableResolved()
        {
            var container = CreateContainer();

            var services = container.GetService<IEnumerable<INonexistentService>>();

            Assert.Empty(services);
        }

        [Fact]
        public void AttemptingToIEnumerableResolveNonexistentServiceIndirectlyThrows()
        {
            var container = CreateContainer();

            // The call to ToArray is necessary for Ninject to throw
            Assert.ThrowsAny<Exception>(() =>
                container.GetService<IEnumerable<IDependOnNonexistentService>>().ToArray());
        }
    }
}
