﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Test
{
    internal static class Factory
    {
        public static IComponentRegistration CreateSingletonRegistration(IEnumerable<Service> services, IInstanceActivator activator)
        {
            return CreateRegistration(services, activator, new RootScopeLifetime(), InstanceSharing.Shared);
        }

        public static IComponentRegistration CreateSingletonRegistration(Type implementation)
        {
            return CreateSingletonRegistration(
                new Service[] { new TypedService(implementation) },
                CreateReflectionActivator(implementation));
        }

        public static IComponentRegistration CreateRegistration(IEnumerable<Service> services, IInstanceActivator activator, IComponentLifetime lifetime, InstanceSharing sharing)
        {
            return new ComponentRegistration(
                Guid.NewGuid(),
                activator,
                lifetime,
                sharing,
                InstanceOwnership.OwnedByLifetimeScope,
                services,
                GetDefaultMetadata());
        }

        public static IComponentRegistration CreateSingletonRegistration<T>(T instance)
        {
            return RegistrationBuilder
                .ForDelegate((c, p) => instance)
                .SingleInstance()
                .CreateRegistration();
        }

        public static IComponentRegistration CreateSingletonObjectRegistration(object instance)
        {
            return CreateSingletonRegistration(instance);
        }

        public static IComponentRegistration CreateSingletonObjectRegistration()
        {
            return CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(object)) },
                CreateReflectionActivator(typeof(object)));
        }

        public static ReflectionActivator CreateReflectionActivator(Type implementation)
        {
            return CreateReflectionActivator(
                implementation,
                NoParameters);
        }

        public static ReflectionActivator CreateReflectionActivator(Type implementation, IEnumerable<Parameter> parameters)
        {
            return CreateReflectionActivator(
                implementation,
                parameters,
                NoProperties);
        }

        public static ReflectionActivator CreateReflectionActivator(Type implementation, IEnumerable<Parameter> parameters, IEnumerable<Parameter> properties)
        {
            return new ReflectionActivator(
                implementation,
                new DefaultConstructorFinder(),
                new MostParametersConstructorSelector(),
                parameters,
                properties);
        }

        public static ProvidedInstanceActivator CreateProvidedInstanceActivator(object instance)
        {
            return new ProvidedInstanceActivator(instance);
        }

        private static IDictionary<string, object> GetDefaultMetadata()
        {
            return new Dictionary<string, object>
            {
                { MetadataKeys.RegistrationOrderMetadataKey, SequenceGenerator.GetNextUniqueSequence() },
            };
        }

        public static IComponentRegistryBuilder CreateEmptyComponentRegistryBuilder()
        {
            return new ComponentRegistryBuilder(new DefaultRegisteredServicesTracker(), new Dictionary<string, object>());
        }

        public static IComponentRegistry CreateEmptyComponentRegistry()
        {
            return CreateEmptyComponentRegistryBuilder().Build();
        }

        public static IContainer CreateEmptyContainer()
        {
            return new ContainerBuilder().Build();
        }

        public static IComponentContext CreateEmptyContext()
        {
            return CreateEmptyContainer();
        }

        public static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

        public static readonly IEnumerable<Parameter> NoProperties = Enumerable.Empty<Parameter>();
    }
}
