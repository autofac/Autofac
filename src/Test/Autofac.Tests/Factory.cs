using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                NoProperties);
        }

        public static IComponentRegistration CreateSingletonObjectRegistration()
        {
            return CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(object)) },
                CreateReflectionActivator(typeof(object)));
        }

        public static ReflectionActivator CreateReflectionActivator(Type implementation)
        {
            return new ReflectionActivator(
                implementation,
                new BindingFlagsConstructorFinder(BindingFlags.Public),
                new MostParametersConstructorSelector(),
                NoParameters);
        }

        public static ProvidedInstanceActivator CreateProvidedInstanceActivator(object instance)
        {
            return new ProvidedInstanceActivator(instance);
        }

        public static readonly IContainer EmptyContainer = new Container();
        public static readonly IComponentContext EmptyContext = new Container();
        public static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();
        public static readonly IDictionary<string, object> NoProperties = new Dictionary<string, object>();
    }
}
