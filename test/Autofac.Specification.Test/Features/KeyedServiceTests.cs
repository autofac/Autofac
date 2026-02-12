// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.AttributeFilters;

namespace Autofac.Specification.Test.Features;

// Tests here mimic the keyed service tests from
// Microsoft.Extensions.DependencyInjection.Specification.Tests.
//
// https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src/KeyedDependencyInjectionSpecificationTests.cs
//
// Some of the tests have been modified or omitted based on what core Autofac
// actually supports.
//
// - Autofac does not support null keys (which is the same as register-by-type).
// - Autofac injects the keyed value into constructors or properties that are
//   explicitly decorated with the ServiceKeyAttribute from Autofac. This is the
//   same concept as the MEDI attribute but not the same literal type.
// - Autofac scopes and injected ILifetimeScope behave slightly differently than
//   MEDI's IServiceScope and injected IServiceProvider, so some of the scope
//   related tests have been omitted or modified.
// - Autofac KeyFilterAttribute/WithAttributeFiltering does not REQUIRE the
//   presence of the keyed service. If the injection can fall back to an
//   unkeyed/typed service, that is allowed.
public class KeyedServiceTests
{
    [Fact]
    public void CombinationalRegistration()
    {
        Service service1 = new();
        Service service2 = new();
        Service keyedService1 = new();
        Service keyedService2 = new();
        Service anyKeyService1 = new();
        Service anyKeyService2 = new();

        var builder = new ContainerBuilder();
        builder.RegisterInstance<IService>(service1);
        builder.RegisterInstance<IService>(service2);
        builder.RegisterInstance<IService>(anyKeyService1).Keyed<IService>(KeyedService.AnyKey);
        builder.RegisterInstance<IService>(anyKeyService2).Keyed<IService>(KeyedService.AnyKey);
        builder.RegisterInstance<IService>(keyedService1).Keyed<IService>("keyedService");
        builder.RegisterInstance<IService>(keyedService2).Keyed<IService>("keyedService");

        var provider = builder.Build();

        /*
         * Table for what results are included:
         *
         * Query                                   | Keyed? | Unkeyed? | AnyKey? |
         * -----------------------------------------------------------------------
         * Resolve<IEnumerable<Type>>              | no     | yes      | no      |
         * Resolve<Type>                           | no     | yes      | no      |
         *
         * ResolveKeyed<IEnumerable<Type>>(AnyKey) | yes    | no       | no      |
         * ResolveKeyed<Type>(AnyKey)              | throw  | throw    | throw   |
         *
         * ResolveKeyed<IEnumerable<Type>>(key)    | yes    | no       | no      |
         * ResolveKeyed<Type>(key)                 | yes    | no       | yes     |
         *
         * Summary:
         * - Autofac does not support null keys, so there is no concept of differentiating between null key and unkeyed.
         * - In MEDI, a null key is the same as unkeyed. This allows their KeyedServices APIs to support both keyed and unkeyed.
         * - AnyKey is a special case of Keyed.
         * - AnyKey registrations are not returned with GetKeyedServices(AnyKey) and GetKeyedService(AnyKey) always throws.
         * - For IEnumerable, the ordering of the results are in registration order.
         * - For a singleton resolve, the last match wins.
         */

        // Unkeyed (register by type).
        Assert.Equal(
            new[] { service1, service2 },
            provider.Resolve<IEnumerable<IService>>());

        Assert.Equal(service2, provider.Resolve<IService>());

        // AnyKey.
        Assert.Equal(
            new[] { keyedService1, keyedService2 },
            provider.ResolveKeyed<IEnumerable<IService>>(KeyedService.AnyKey));

        Assert.Throws<DependencyResolutionException>(() => provider.ResolveKeyed<IService>(KeyedService.AnyKey));

        // Keyed.
        Assert.Equal(
            new[] { keyedService1, keyedService2 },
            provider.ResolveKeyed<IEnumerable<IService>>("keyedService"));

        Assert.Equal(keyedService2, provider.ResolveKeyed<IService>("keyedService"));
    }

    [Fact]
    public void ResolveKeyedService()
    {
        var service1 = new Service();
        var service2 = new Service();
        var builder = new ContainerBuilder();
        builder.RegisterInstance<IService>(service1).Keyed<IService>("service1");
        builder.RegisterInstance<IService>(service2).Keyed<IService>("service2");

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Same(service1, provider.ResolveKeyed<IService>("service1"));
        Assert.Same(service2, provider.ResolveKeyed<IService>("service2"));

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve(typeof(IService)));
        Assert.Same(service1, provider.ResolveKeyed("service1", typeof(IService)));
        Assert.Same(service2, provider.ResolveKeyed("service2", typeof(IService)));
    }

    [Fact]
    public void ResolveKeyedOpenGenericService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(FakeOpenGenericService<>)).Keyed("my-service", typeof(IFakeOpenGenericService<>));
        builder.RegisterType<FakeService>().As<IFakeSingletonService>().SingleInstance();
        var provider = builder.Build();

        // Act
        var genericService = provider.ResolveKeyed<IFakeOpenGenericService<IFakeSingletonService>>("my-service");
        var singletonService = provider.Resolve<IFakeSingletonService>();

        // Assert
        Assert.Same(singletonService, genericService.Value);
    }

    [Fact]
    public void ResolveKeyedServices()
    {
        var service1 = new Service();
        var service2 = new Service();
        var service3 = new Service();
        var service4 = new Service();
        var builder = new ContainerBuilder();
        builder.RegisterInstance<IService>(service1).Keyed<IService>("first-service");
        builder.RegisterInstance<IService>(service2).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service3).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service4).Keyed<IService>("service");

        var provider = builder.Build();

        var firstSvc = provider.ResolveKeyed<IEnumerable<IService>>("first-service").ToList();
        Assert.Single(firstSvc);
        Assert.Same(service1, firstSvc[0]);

        var services = provider.ResolveKeyed<IEnumerable<IService>>("service").ToList();
        Assert.Equal(new[] { service2, service3, service4 }, services);
    }

    [Fact]
    public void ResolveKeyedServicesAnyKey()
    {
        var service1 = new Service();
        var service2 = new Service();
        var service3 = new Service();
        var service4 = new Service();
        var service5 = new Service();
        var service6 = new Service();
        var builder = new ContainerBuilder();
        builder.RegisterInstance<IService>(service1).Keyed<IService>("first-service");
        builder.RegisterInstance<IService>(service2).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service3).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service4).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service5);
        builder.RegisterInstance<IService>(service6);

        var provider = builder.Build();

        // Return all services registered with a non null key
        var allServices = provider.ResolveKeyed<IEnumerable<IService>>(KeyedService.AnyKey).ToList();
        Assert.Equal(4, allServices.Count);
        Assert.Equal(new[] { service1, service2, service3, service4 }, allServices);

        // Check again (caching)
        var allServices2 = provider.ResolveKeyed<IEnumerable<IService>>(KeyedService.AnyKey).ToList();
        Assert.Equal(allServices, allServices2);
    }

    [Fact]
    public void ResolveKeyedServicesAnyKeyWithAnyKeyRegistration()
    {
        var service1 = new Service();
        var service2 = new Service();
        var service3 = new Service();
        var service4 = new Service();
        var service5 = new Service();
        var service6 = new Service();
        var builder = new ContainerBuilder();
        builder.Register<IService>(ctx => new Service()).Keyed<IService>(KeyedService.AnyKey);
        builder.RegisterInstance<IService>(service1).Keyed<IService>("first-service");
        builder.RegisterInstance<IService>(service2).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service3).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service4).Keyed<IService>("service");
        builder.RegisterInstance<IService>(service5);
        builder.RegisterInstance<IService>(service6);

        var provider = builder.Build();

        _ = provider.ResolveKeyed<IService>("something-else");
        _ = provider.ResolveKeyed<IService>("something-else-again");

        // Return all services registered with a non null key, but not the one "created" with KeyedService.AnyKey,
        // nor the KeyedService.AnyKey registration
        var allServices = provider.ResolveKeyed<IEnumerable<IService>>(KeyedService.AnyKey).ToList();
        Assert.Equal(4, allServices.Count);
        Assert.Equal(new[] { service1, service2, service3, service4 }, allServices);

        var someKeyedServices = provider.ResolveKeyed<IEnumerable<IService>>("service").ToList();
        Assert.Equal(new[] { service2, service3, service4 }, someKeyedServices);

        var unkeyedServices = provider.Resolve<IEnumerable<IService>>().ToList();
        Assert.Equal(new[] { service5, service6 }, unkeyedServices);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveWithAnyKeyQuery_Constructor(bool anyKeyQueryBeforeSingletonQueries)
    {
        // Test ordering and slot assignments when DI calls the service's constructor
        // across keyed services with different service types and keys.
        var builder = new ContainerBuilder();

        // Interweave these to check that the slot \ ordering logic is correct.
        // Each unique key + its service Type maintains their own slot in a AnyKey query.
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key1").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key1").SingleInstance();
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key2").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key2").SingleInstance();
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key3").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key3").SingleInstance();

        var provider = builder.Build();

        TestServiceA[] allInstancesA = null;
        TestServiceB[] allInstancesB = null;

        if (anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        var serviceA1 = provider.ResolveKeyed<TestServiceA>("key1");
        var serviceB1 = provider.ResolveKeyed<TestServiceB>("key1");
        var serviceA2 = provider.ResolveKeyed<TestServiceA>("key2");
        var serviceB2 = provider.ResolveKeyed<TestServiceB>("key2");
        var serviceA3 = provider.ResolveKeyed<TestServiceA>("key3");
        var serviceB3 = provider.ResolveKeyed<TestServiceB>("key3");

        if (!anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        Assert.Equal(
            new[] { serviceA1, serviceA2, serviceA3 },
            allInstancesA);

        Assert.Equal(
            new[] { serviceB1, serviceB2, serviceB3 },
            allInstancesB);

        void DoAnyKeyQuery()
        {
            IEnumerable<TestServiceA> allA = provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey);
            IEnumerable<TestServiceB> allB = provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey);

            // Verify caching returns the same IEnumerable<> instance.
            Assert.Same(allA, provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey));
            Assert.Same(allB, provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey));

            allInstancesA = allA.ToArray();
            allInstancesB = allB.ToArray();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveWithAnyKeyQuery_Constructor_Duplicates(bool anyKeyQueryBeforeSingletonQueries)
    {
        // Test ordering and slot assignments when DI calls the service's constructor
        // across keyed services with different service types with duplicate keys.
        var builder = new ContainerBuilder();

        // Interweave these to check that the slot \ ordering logic is correct.
        // Each unique key + its service Type maintains their own slot in a AnyKey query.
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key").SingleInstance();
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key").SingleInstance();
        builder.RegisterType<TestServiceA>().Keyed<TestServiceA>("key").SingleInstance();
        builder.RegisterType<TestServiceB>().Keyed<TestServiceB>("key").SingleInstance();

        var provider = builder.Build();

        TestServiceA[] allInstancesA = null;
        TestServiceB[] allInstancesB = null;

        if (anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        var serviceA = provider.ResolveKeyed<TestServiceA>("key");
        Assert.Same(serviceA, provider.ResolveKeyed<TestServiceA>("key"));

        var serviceB = provider.ResolveKeyed<TestServiceB>("key");
        Assert.Same(serviceB, provider.ResolveKeyed<TestServiceB>("key"));

        if (!anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        // An AnyKey query we get back the last registered service for duplicates.
        // The first and second services are effectively hidden unless we query all.
        Assert.Equal(3, allInstancesA.Length);
        Assert.Same(serviceA, allInstancesA[2]);
        Assert.NotSame(serviceA, allInstancesA[1]);
        Assert.NotSame(serviceA, allInstancesA[0]);
        Assert.NotSame(allInstancesA[0], allInstancesA[1]);

        Assert.Equal(3, allInstancesB.Length);
        Assert.Same(serviceB, allInstancesB[2]);
        Assert.NotSame(serviceB, allInstancesB[1]);
        Assert.NotSame(serviceB, allInstancesB[0]);
        Assert.NotSame(allInstancesB[0], allInstancesB[1]);

        void DoAnyKeyQuery()
        {
            IEnumerable<TestServiceA> allA = provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey);
            IEnumerable<TestServiceB> allB = provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey);

            // Verify caching returns the same IEnumerable<> instances.
            Assert.Same(allA, provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey));
            Assert.Same(allB, provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey));

            allInstancesA = allA.ToArray();
            allInstancesB = allB.ToArray();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveWithAnyKeyQuery_InstanceProvided(bool anyKeyQueryBeforeSingletonQueries)
    {
        // Test ordering and slot assignments when service is provided
        // across keyed services with different service types and keys.
        var builder = new ContainerBuilder();

        TestServiceA serviceA1 = new();
        TestServiceA serviceA2 = new();
        TestServiceA serviceA3 = new();
        TestServiceB serviceB1 = new();
        TestServiceB serviceB2 = new();
        TestServiceB serviceB3 = new();

        // Interweave these to check that the slot \ ordering logic is correct.
        // Each unique key + its service Type maintains their own slot in a AnyKey query.
        builder.RegisterInstance(serviceA1).Keyed<TestServiceA>("key1");
        builder.RegisterInstance(serviceB1).Keyed<TestServiceB>("key1");
        builder.RegisterInstance(serviceA2).Keyed<TestServiceA>("key2");
        builder.RegisterInstance(serviceB2).Keyed<TestServiceB>("key2");
        builder.RegisterInstance(serviceA3).Keyed<TestServiceA>("key3");
        builder.RegisterInstance(serviceB3).Keyed<TestServiceB>("key3");

        var provider = builder.Build();

        TestServiceA[] allInstancesA = null;
        TestServiceB[] allInstancesB = null;

        if (anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        var fromServiceA1 = provider.ResolveKeyed<TestServiceA>("key1");
        var fromServiceA2 = provider.ResolveKeyed<TestServiceA>("key2");
        var fromServiceA3 = provider.ResolveKeyed<TestServiceA>("key3");
        Assert.Same(serviceA1, fromServiceA1);
        Assert.Same(serviceA2, fromServiceA2);
        Assert.Same(serviceA3, fromServiceA3);

        var fromServiceB1 = provider.ResolveKeyed<TestServiceB>("key1");
        var fromServiceB2 = provider.ResolveKeyed<TestServiceB>("key2");
        var fromServiceB3 = provider.ResolveKeyed<TestServiceB>("key3");
        Assert.Same(serviceB1, fromServiceB1);
        Assert.Same(serviceB2, fromServiceB2);
        Assert.Same(serviceB3, fromServiceB3);

        if (!anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        Assert.Equal(
            new[] { serviceA1, serviceA2, serviceA3 },
            allInstancesA);

        Assert.Equal(
            new[] { serviceB1, serviceB2, serviceB3 },
            allInstancesB);

        void DoAnyKeyQuery()
        {
            IEnumerable<TestServiceA> allA = provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey);
            IEnumerable<TestServiceB> allB = provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey);

            // Verify caching returns the same items.
            Assert.Equal(allA, provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey));
            Assert.Equal(allB, provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey));

            allInstancesA = allA.ToArray();
            allInstancesB = allB.ToArray();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveWithAnyKeyQuery_InstanceProvided_Duplicates(bool anyKeyQueryBeforeSingletonQueries)
    {
        // Test ordering and slot assignments when service is provided
        // across keyed services with different service types with duplicate keys.
        var builder = new ContainerBuilder();

        TestServiceA serviceA1 = new();
        TestServiceA serviceA2 = new();
        TestServiceA serviceA3 = new();
        TestServiceB serviceB1 = new();
        TestServiceB serviceB2 = new();
        TestServiceB serviceB3 = new();

        // Interweave these to check that the slot \ ordering logic is correct.
        // Each unique key + its service Type maintains their own slot in a AnyKey query.
        builder.RegisterInstance(serviceA1).Keyed<TestServiceA>("key");
        builder.RegisterInstance(serviceB1).Keyed<TestServiceB>("key");
        builder.RegisterInstance(serviceA2).Keyed<TestServiceA>("key");
        builder.RegisterInstance(serviceB2).Keyed<TestServiceB>("key");
        builder.RegisterInstance(serviceA3).Keyed<TestServiceA>("key");
        builder.RegisterInstance(serviceB3).Keyed<TestServiceB>("key");

        var provider = builder.Build();

        TestServiceA[] allInstancesA = null;
        TestServiceB[] allInstancesB = null;

        if (anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        // We get back the last registered service for duplicates.
        Assert.Same(serviceA3, provider.ResolveKeyed<TestServiceA>("key"));
        Assert.Same(serviceB3, provider.ResolveKeyed<TestServiceB>("key"));

        if (!anyKeyQueryBeforeSingletonQueries)
        {
            DoAnyKeyQuery();
        }

        Assert.Equal(
            new[] { serviceA1, serviceA2, serviceA3 },
            allInstancesA);

        Assert.Equal(
            new[] { serviceB1, serviceB2, serviceB3 },
            allInstancesB);

        void DoAnyKeyQuery()
        {
            IEnumerable<TestServiceA> allA = provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey);
            IEnumerable<TestServiceB> allB = provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey);

            // Verify caching returns the same items.
            Assert.Equal(allA, provider.ResolveKeyed<IEnumerable<TestServiceA>>(KeyedService.AnyKey));
            Assert.Equal(allB, provider.ResolveKeyed<IEnumerable<TestServiceB>>(KeyedService.AnyKey));

            allInstancesA = allA.ToArray();
            allInstancesB = allB.ToArray();
        }
    }

    private class TestServiceA
    {
    }

    private class TestServiceB
    {
    }

    [Fact]
    public void ResolveKeyedServicesAnyKeyOrdering()
    {
        var builder = new ContainerBuilder();
        var service1 = new Service();
        var service2 = new Service();
        var service3 = new Service();

        builder.RegisterInstance<IService>(service1).Keyed<IService>("A-service");
        builder.RegisterInstance<IService>(service2).Keyed<IService>("B-service");
        builder.RegisterInstance<IService>(service3).Keyed<IService>("A-service");

        var provider = builder.Build();

        // The order should be in registration order, and not grouped by key for example.
        // Although this isn't necessarily a requirement, it is the current behavior.
        Assert.Equal(
            new[] { service1, service2, service3 },
            provider.ResolveKeyed<IEnumerable<IService>>(KeyedService.AnyKey));
    }

    [Fact]
    public void ResolveKeyedGenericServices()
    {
        var service1 = new FakeService();
        var service2 = new FakeService();
        var service3 = new FakeService();
        var service4 = new FakeService();
        var builder = new ContainerBuilder();
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service1).Keyed<IFakeOpenGenericService<PocoClass>>("first-service");
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service2).Keyed<IFakeOpenGenericService<PocoClass>>("service");
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service3).Keyed<IFakeOpenGenericService<PocoClass>>("service");
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service4).Keyed<IFakeOpenGenericService<PocoClass>>("service");

        var provider = builder.Build();

        var firstSvc = provider.ResolveKeyed<IEnumerable<IFakeOpenGenericService<PocoClass>>>("first-service").ToList();
        Assert.Single(firstSvc);
        Assert.Same(service1, firstSvc[0]);

        var services = provider.ResolveKeyed<IEnumerable<IFakeOpenGenericService<PocoClass>>>("service").ToList();
        Assert.Equal(new[] { service2, service3, service4 }, services);
    }

    [Fact]
    public void ResolveKeyedServiceSingletonInstance()
    {
        var service = new Service();
        var builder = new ContainerBuilder();
        builder.RegisterInstance<IService>(service).Keyed<IService>("service1");

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Same(service, provider.ResolveKeyed<IService>("service1"));
        Assert.Same(service, provider.ResolveKeyed("service1", typeof(IService)));
    }

    [Fact]
    public void ResolveKeyedServiceSingletonInstanceWithKeyInjection()
    {
        var serviceKey = "this-is-my-service";
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>(serviceKey).SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        var svc = provider.ResolveKeyed<IService>(serviceKey);
        Assert.NotNull(svc);
        Assert.Equal(serviceKey, svc.ToString());
    }

    [Fact]
    public void ResolveKeyedServiceOpenGenericWithKeyInjection()
    {
        var serviceKey = "this-is-my-service";
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(KeyAwareGenericService<>)).Keyed(serviceKey, typeof(KeyAwareGenericService<>));
        builder.RegisterType<PocoClass>();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<KeyAwareGenericService<PocoClass>>());
        var svc = provider.ResolveKeyed<KeyAwareGenericService<PocoClass>>(serviceKey);
        Assert.NotNull(svc);
        Assert.Equal(serviceKey, svc.Key);
    }

    [Fact]
    public void ResolveKeyedServiceSingletonInstanceWithKeyPropertyInjection()
    {
        var serviceKey = "this-is-my-service";
        var builder = new ContainerBuilder();
        builder.RegisterType<KeyAwarePropertyService>().Keyed<KeyAwarePropertyService>(serviceKey).PropertiesAutowired().SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<KeyAwarePropertyService>());
        var svc = provider.ResolveKeyed<KeyAwarePropertyService>(serviceKey);
        Assert.NotNull(svc);
        Assert.Equal(serviceKey, svc.Key);
    }

    [Fact]
    public void ResolveKeyedServiceSingletonInstanceWithAnyKey()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>(KeyedService.AnyKey).SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());

        var serviceKey1 = "some-key";
        var svc1 = provider.ResolveKeyed<IService>(serviceKey1);
        Assert.NotNull(svc1);
        Assert.Equal(serviceKey1, svc1.ToString());

        var serviceKey2 = "some-other-key";
        var svc2 = provider.ResolveKeyed<IService>(serviceKey2);
        Assert.NotNull(svc2);
        Assert.Equal(serviceKey2, svc2.ToString());
    }

    [Fact]
    public void ResolveKeyedServicesSingletonInstanceWithAnyKey()
    {
        var service1 = new FakeService();
        var service2 = new FakeService();

        var builder = new ContainerBuilder();
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service1).Keyed<IFakeOpenGenericService<PocoClass>>(KeyedService.AnyKey);
        builder.RegisterInstance<IFakeOpenGenericService<PocoClass>>(service2).Keyed<IFakeOpenGenericService<PocoClass>>("some-key");

        var provider = builder.Build();

        var services = provider.ResolveKeyed<IEnumerable<IFakeOpenGenericService<PocoClass>>>("some-key").ToList();
        Assert.Equal(new[] { service2 }, services);
    }

    [Fact]
    public void ResolveKeyedServiceSingletonInstanceWithKeyedParameter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("service1").SingleInstance();
        builder.RegisterType<Service>().Keyed<IService>("service2").SingleInstance();
        builder.RegisterType<OtherService>().SingleInstance().WithAttributeFiltering();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        var svc = provider.Resolve<OtherService>();
        Assert.NotNull(svc);
        Assert.Equal("service1", svc.Service1.ToString());
        Assert.Equal("service2", svc.Service2.ToString());
    }

    [Fact]
    public void ResolveKeyedServiceWithKeyedParameter_MissingRegistration_SecondParameter()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<Service>().Keyed<IService>("service1").SingleInstance();

        // We are missing the registration for "service2" here and OtherService requires it.
        builder.RegisterType<OtherService>().SingleInstance().WithAttributeFiltering();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Throws<DependencyResolutionException>(() => provider.Resolve<OtherService>());
    }

    [Fact]
    public void ResolveKeyedServiceWithKeyedParameter_MissingRegistration_FirstParameter()
    {
        var builder = new ContainerBuilder();

        // We are not registering "service1" and "service2" keyed IService services and OtherService requires them.
        builder.RegisterType<OtherService>().SingleInstance().WithAttributeFiltering();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Throws<DependencyResolutionException>(() => provider.Resolve<OtherService>());
    }

    [Fact]
    public void ResolveKeyedServiceWithKeyedParameter_MissingRegistrationButWithDefaults()
    {
        var builder = new ContainerBuilder();

        // We are not registering "service1" and "service2" keyed IService services and OtherServiceWithDefaultCtorArgs
        // specifies them but has argument defaults if missing.
        builder.RegisterType<OtherServiceWithDefaultCtorArgs>().SingleInstance().WithAttributeFiltering();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.NotNull(provider.Resolve<OtherServiceWithDefaultCtorArgs>());
    }

    [Fact]
    public void ResolveKeyedServiceSingletonFactory()
    {
        var service = new Service();
        var builder = new ContainerBuilder();
        builder.Register<IService>(ctx => service).Keyed<IService>("service1").SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Same(service, provider.ResolveKeyed<IService>("service1"));
        Assert.Same(service, provider.ResolveKeyed("service1", typeof(IService)));
    }

    [Fact]
    public void ResolveKeyedServiceSingletonFactoryWithAnyKey()
    {
        var builder = new ContainerBuilder();
        builder
            .Register<IService>((ctx, p) =>
            {
                var key = p.KeyedServiceKey<string>();
                return new Service(key);
            })
            .Keyed<IService>(KeyedService.AnyKey)
            .SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());

        for (int i = 0; i < 3; i++)
        {
            var key = "service" + i;
            var s1 = provider.ResolveKeyed<IService>(key);
            var s2 = provider.ResolveKeyed<IService>(key);
            Assert.Same(s1, s2);
            Assert.Equal(key, s1.ToString());
        }
    }

    [Fact]
    public void ResolveKeyedServiceSingletonType()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("service1").SingleInstance();

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        Assert.Equal(typeof(Service), provider.ResolveKeyed<IService>("service1").GetType());
    }

    [Fact]
    public void ResolveKeyedServiceTransientFactory()
    {
        var builder = new ContainerBuilder();
        builder
            .Register<IService>((ctx, p) =>
            {
                var key = p.KeyedServiceKey<string>();
                return new Service(key);
            })
            .Keyed<IService>("service1");

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        var first = provider.ResolveKeyed<IService>("service1");
        var second = provider.ResolveKeyed<IService>("service1");
        Assert.NotSame(first, second);
        Assert.Equal("service1", first.ToString());
        Assert.Equal("service1", second.ToString());
    }

    [Fact]
    public void ResolveKeyedServiceTransientType()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("service1");

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        var first = provider.ResolveKeyed<IService>("service1");
        var second = provider.ResolveKeyed<IService>("service1");
        Assert.NotSame(first, second);
    }

    [Fact]
    public void ResolveKeyedServiceTransientTypeWithAnyKey()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>(KeyedService.AnyKey);

        var provider = builder.Build();

        Assert.Throws<ComponentNotRegisteredException>(() => provider.Resolve<IService>());
        var first = provider.ResolveKeyed<IService>("service1");
        var second = provider.ResolveKeyed<IService>("service1");
        Assert.NotSame(first, second);
    }

    [Fact]
    public void ResolveKeyedSingletonFromScopeServiceProvider()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("key").SingleInstance();

        var provider = builder.Build();
        var scopeA = provider.BeginLifetimeScope();
        var scopeB = provider.BeginLifetimeScope();

        Assert.Throws<ComponentNotRegisteredException>(() => scopeA.Resolve<IService>());
        Assert.Throws<ComponentNotRegisteredException>(() => scopeB.Resolve<IService>());

        Assert.Throws<DependencyResolutionException>(() => scopeA.ResolveKeyed<IService>(KeyedService.AnyKey));
        Assert.Throws<DependencyResolutionException>(() => scopeB.ResolveKeyed<IService>(KeyedService.AnyKey));

        var serviceA1 = scopeA.ResolveKeyed<IService>("key");
        var serviceA2 = scopeA.ResolveKeyed<IService>("key");

        var serviceB1 = scopeB.ResolveKeyed<IService>("key");
        var serviceB2 = scopeB.ResolveKeyed<IService>("key");

        Assert.Same(serviceA1, serviceA2);
        Assert.Same(serviceB1, serviceB2);
        Assert.Same(serviceA1, serviceB1);
    }

    [Fact]
    public void ResolveKeyedScopedFromScopeServiceProvider()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("key").InstancePerLifetimeScope();

        var provider = builder.Build();
        var scopeA = provider.BeginLifetimeScope();
        var scopeB = provider.BeginLifetimeScope();

        Assert.Throws<ComponentNotRegisteredException>(() => scopeA.Resolve<IService>());
        Assert.Throws<ComponentNotRegisteredException>(() => scopeB.Resolve<IService>());

        Assert.Throws<DependencyResolutionException>(() => scopeA.ResolveKeyed<IService>(KeyedService.AnyKey));
        Assert.Throws<DependencyResolutionException>(() => scopeB.ResolveKeyed<IService>(KeyedService.AnyKey));

        var serviceA1 = scopeA.ResolveKeyed<IService>("key");
        var serviceA2 = scopeA.ResolveKeyed<IService>("key");

        var serviceB1 = scopeB.ResolveKeyed<IService>("key");
        var serviceB2 = scopeB.ResolveKeyed<IService>("key");

        Assert.Same(serviceA1, serviceA2);
        Assert.Same(serviceB1, serviceB2);
        Assert.NotSame(serviceA1, serviceB1);
    }

    [Fact]
    public void ResolveKeyedTransientFromScopeServiceProvider()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Service>().Keyed<IService>("key");

        var provider = builder.Build();
        var scopeA = provider.BeginLifetimeScope();
        var scopeB = provider.BeginLifetimeScope();

        Assert.Throws<ComponentNotRegisteredException>(() => scopeA.Resolve<IService>());
        Assert.Throws<ComponentNotRegisteredException>(() => scopeB.Resolve<IService>());

        var serviceA1 = scopeA.ResolveKeyed<IService>("key");
        var serviceA2 = scopeA.ResolveKeyed<IService>("key");

        var serviceB1 = scopeB.ResolveKeyed<IService>("key");
        var serviceB2 = scopeB.ResolveKeyed<IService>("key");

        Assert.NotSame(serviceA1, serviceA2);
        Assert.NotSame(serviceB1, serviceB2);
        Assert.NotSame(serviceA1, serviceB1);
    }

    [Fact]
    public void ResolveRequiredKeyedServiceThrowsIfNotFound()
    {
        var builder = new ContainerBuilder();
        var provider = builder.Build();
        var serviceKey = new object();

        ComponentNotRegisteredException e;

        e = Assert.Throws<ComponentNotRegisteredException>(() => provider.ResolveKeyed<IService>(serviceKey));
        VerifyException();

        e = Assert.Throws<ComponentNotRegisteredException>(() => provider.ResolveKeyed(serviceKey, typeof(IService)));
        VerifyException();

        void VerifyException()
        {
            Assert.Contains(nameof(IService), e.Message, StringComparison.Ordinal);
            Assert.Contains(serviceKey.GetType().FullName, e.Message, StringComparison.Ordinal);
        }
    }

    private interface IService
    {
    }

    private class Service : IService
    {
        private readonly string _id;

        public Service() => _id = Guid.NewGuid().ToString();

        public Service([ServiceKey] string id) => _id = id;

        public override string ToString() => _id;
    }

    private class OtherService
    {
        public OtherService(
            [KeyFilter("service1")] IService service1,
            [KeyFilter("service2")] IService service2)
        {
            Service1 = service1;
            Service2 = service2;
        }

        public IService Service1 { get; }

        public IService Service2 { get; }
    }

    private class OtherServiceWithDefaultCtorArgs
    {
        public OtherServiceWithDefaultCtorArgs(
            [KeyFilter("service1")] IService service1 = null,
            [KeyFilter("service2")] IService service2 = null)
        {
            Service1 = service1;
            Service2 = service2;
        }

        public IService Service1 { get; }

        public IService Service2 { get; }
    }

    [Fact]
    public void SimpleServiceKeyedResolution()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterType<SimpleService>().Keyed<ISimpleService>("simple");
        builder.RegisterType<AnotherSimpleService>().Keyed<ISimpleService>("another");
        builder.RegisterType<SimpleParentWithDynamicKeyedService>();
        var provider = builder.Build();
        var sut = provider.Resolve<SimpleParentWithDynamicKeyedService>();

        // Act
        var result = sut.GetService("simple");

        // Assert
        Assert.True(result.GetType() == typeof(SimpleService));
    }

    private class KeyAwarePropertyService
    {
        [ServiceKey]
        public object Key { get; set; } = default!;
    }

    private class KeyAwareGenericService<T>
    {
        public KeyAwareGenericService([ServiceKey] string key, T value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; } = default!;

        public T Value { get; set; } = default!;
    }

    private class SimpleParentWithDynamicKeyedService
    {
        private readonly ILifetimeScope _lifetimeScope;

        public SimpleParentWithDynamicKeyedService(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public ISimpleService GetService(string name) => _lifetimeScope.ResolveKeyed<ISimpleService>(name);
    }

    private interface ISimpleService
    {
    }

    private class SimpleService : ISimpleService
    {
    }

    private class AnotherSimpleService : ISimpleService
    {
    }

    private class FakeService : IFakeSingletonService, IFakeOpenGenericService<PocoClass>
    {
        public PocoClass Value { get; set; }
    }

    private interface IFakeSingletonService
    {
    }

    private interface IFakeOpenGenericService<out TValue>
    {
        TValue Value { get; }
    }

    private class PocoClass
    {
    }

    private class FakeOpenGenericService<TVal> : IFakeOpenGenericService<TVal>
    {
        public FakeOpenGenericService(TVal value)
        {
            Value = value;
        }

        public TVal Value { get; }
    }
}
