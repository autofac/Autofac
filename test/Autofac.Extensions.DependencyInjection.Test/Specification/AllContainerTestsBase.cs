// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public abstract class AllContainerTestsBase
    {
        protected abstract IServiceProvider CreateContainer();

        [Fact]
        public void SingleServiceCanBeResolved()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeService>();

            Assert.NotNull(service);
            Assert.Equal("FakeServiceSimpleMethod", service.SimpleMethod());
        }

        public void ServiceInstanceCanBeResolved()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeServiceInstance>();

            Assert.NotNull(service);
            Assert.Equal("Instance", service.SimpleMethod());
        }

        [Fact]
        public void TransientServiceCanBeResolved()
        {
            var container = CreateContainer();

            var service1 = container.GetService<IFakeService>();
            var service2 = container.GetService<IFakeService>();

            Assert.NotNull(service1);
            Assert.NotEqual(service1, service2);
        }

        [Fact]
        public void SingleServiceCanBeIEnumerableResolved()
        {
            var container = CreateContainer();

            var services = container.GetService<IEnumerable<IFakeService>>();

            Assert.NotNull(services);
            Assert.Equal(1, services.Count());
            Assert.Equal("FakeServiceSimpleMethod", services.Single().SimpleMethod());
        }

        [Fact]
        public void MultipleServiceCanBeIEnumerableResolved()
        {
            var container = CreateContainer();

            var services = container.GetService<IEnumerable<IFakeMultipleService>>();

            var results = services.Select(x => x.SimpleMethod()).ToArray();

            Assert.NotNull(results);
            Assert.Equal(2, results.Count());
            Assert.Contains("FakeOneMultipleServiceAnotherMethod", results);
            Assert.Contains("FakeTwoMultipleServiceAnotherMethod", results);
        }

        [Fact]
        public void OuterServiceCanHaveOtherServicesInjected()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeOuterService>();

            string singleValue;
            string[] multipleValues;
            service.Interrogate(out singleValue, out multipleValues);

            Assert.NotNull(service);
            Assert.Equal(2, multipleValues.Count());
            Assert.Contains("FakeServiceSimpleMethod", singleValue);
            Assert.Contains("FakeOneMultipleServiceAnotherMethod", multipleValues);
            Assert.Contains("FakeTwoMultipleServiceAnotherMethod", multipleValues);
        }

        [Fact]
        public void FactoryServicesCanBeCreatedByGetService()
        {
            // Arrange
            var container = CreateContainer();

            // Act
            var service = container.GetService<IFactoryService>();

            // Assert
            Assert.Equal(42, service.Value);
            Assert.NotNull(service.FakeService);
        }

        [Fact]
        public void FactoryServicesAreCreatedAsPartOfCreatingObjectGraph()
        {
            // Arrange
            var container = CreateContainer();

            // Act
            var service1 = container.GetService<ServiceAcceptingFactoryService>();
            var service2 = container.GetService<ServiceAcceptingFactoryService>();

            // Assert
            Assert.Equal(42, service1.TransientService.Value);
            Assert.NotNull(service1.TransientService.FakeService);

            Assert.Equal(42, service2.TransientService.Value);
            Assert.NotNull(service2.TransientService.FakeService);

            Assert.NotNull(service1.ScopedService.FakeService);

            // Verify scoping works
            Assert.NotSame(service1.TransientService, service2.TransientService);
            Assert.Same(service1.ScopedService, service2.ScopedService);
        }
    }
}
