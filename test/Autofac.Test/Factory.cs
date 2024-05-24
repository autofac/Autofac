// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Test;

internal static class Factory
{
    public static IComponentRegistration CreateSingletonRegistration(IEnumerable<Service> services, IInstanceActivator activator)
    {
        return CreateRegistration(services, activator, RootScopeLifetime.Instance, InstanceSharing.Shared);
    }

    [SuppressMessage("CA2000", "CA2000", Justification = "Disposing the registration will dispose the activator.")]
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

    [SuppressMessage("CA2000", "CA2000", Justification = "Disposing the registration will dispose the activator.")]
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

    public static ReflectionActivator CreateReflectionActivator(Type implementation, IConstructorFinder customFinder)
    {
        return new ReflectionActivator(
            implementation,
            customFinder,
            new MostParametersConstructorSelector(),
            NoParameters,
            NoProperties);
    }

    public static ReflectionActivator CreateReflectionActivator(Type implementation, IConstructorSelector customSelector)
    {
        return new ReflectionActivator(
            implementation,
            new DefaultConstructorFinder(),
            customSelector,
            NoParameters,
            NoProperties);
    }

    public static ProvidedInstanceActivator CreateProvidedInstanceActivator(object instance)
    {
        return new ProvidedInstanceActivator(instance);
    }

    private static Dictionary<string, object> GetDefaultMetadata()
    {
        return new Dictionary<string, object>
            {
                { MetadataKeys.RegistrationOrderMetadataKey, SequenceGenerator.GetNextUniqueSequence() },
            };
    }

    [SuppressMessage("CA2000", "CA2000", Justification = "The component registry builder handles disposal of the services tracker.")]
    public static IComponentRegistryBuilder CreateEmptyComponentRegistryBuilder()
    {
        return new ComponentRegistryBuilder(new DefaultRegisteredServicesTracker(), new Dictionary<string, object>());
    }

    [SuppressMessage("CA2000", "CA2000", Justification = "Shortcut for testing.")]
    public static IComponentRegistry CreateEmptyComponentRegistry()
    {
        // Re: disposal - technically speaking, this is an odd situation.
        // Normally what happens is a lifetime scope will take the builder, run
        // build, and then add the builder to the list of things that get
        // disposed at the end of the lifetime scope along with the built
        // registry. In this test situation, there really isn't a scope, but if
        // we dispose the builder we'll also end up disposing things inside the
        // registry that got built, like the registered services tracker.
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
