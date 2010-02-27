using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Core.Registration;
using Autofac.Core.Lifetime;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;
using System.Reflection;
using Autofac.Core.Activators.ProvidedInstance;

namespace Autofac.Tests
{
    static class Factory
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
                NoMetadata);
        }

        public static IComponentRegistration CreateSingletonObjectRegistration(object instance)
        {
            return RegistrationBuilder
                .ForDelegate((c, p) => instance)
                .SingleInstance()
                .CreateRegistration();
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
                new BindingFlagsConstructorFinder(BindingFlags.Public),
                new MostParametersConstructorSelector(),
                parameters,
                properties);
        }

        public static ProvidedInstanceActivator CreateProvidedInstanceActivator(object instance)
        {
            return new ProvidedInstanceActivator(instance);
        }

        public static readonly IContainer EmptyContainer = new Container();
        public static readonly IComponentContext EmptyContext = new Container();
        public static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();
        public static readonly IEnumerable<Parameter> NoProperties = Enumerable.Empty<Parameter>();
        public static readonly IDictionary<string, object> NoMetadata = new Dictionary<string, object>();
    }
}
