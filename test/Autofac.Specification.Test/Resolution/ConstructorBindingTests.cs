// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Specification.Test.Resolution;

// Constructor arguments must bind against the registrations visible where the component is resolved, every time. A
// binding cache that kept more than the argument type would break these.
public class ConstructorBindingTests
{
    private interface IDependency
    {
    }

    private interface ITrigger
    {
    }

    [Fact]
    public void SiblingScopesEachBindTheirOwnDefault()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Consumer>();
        builder.RegisterType<Dependency1>().As<IDependency>();
        using var container = builder.Build();

        Assert.IsType<Dependency1>(container.Resolve<Consumer>().Dependency);

        using var scope1 = container.BeginLifetimeScope(b => b.RegisterType<Dependency2>().As<IDependency>());
        using var scope2 = container.BeginLifetimeScope(b => b.RegisterType<Dependency3>().As<IDependency>());

        Assert.IsType<Dependency2>(scope1.Resolve<Consumer>().Dependency);
        Assert.IsType<Dependency3>(scope2.Resolve<Consumer>().Dependency);
        Assert.IsType<Dependency1>(container.Resolve<Consumer>().Dependency);
    }

    [Fact]
    public void DefaultAddedByARegistrationSourceAfterTheFirstResolveIsBound()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Consumer>();
        builder.RegisterType<Dependency1>().As<IDependency>().PreserveExistingDefaults();
        builder.RegisterSource(new TriggeredDependencySource());
        using var container = builder.Build();

        Assert.IsType<Dependency1>(container.Resolve<Consumer>().Dependency);

        // The source registers Dependency2 for both services, which outranks a PreserveExistingDefaults registration.
        container.Resolve<ITrigger>();

        Assert.IsType<Dependency2>(container.Resolve<Consumer>().Dependency);
    }

    private class Consumer
    {
        public Consumer(IDependency dependency)
        {
            Dependency = dependency;
        }

        public IDependency Dependency
        {
            get;
        }
    }

    private class Dependency1 : IDependency
    {
    }

    private class Dependency2 : IDependency, ITrigger
    {
    }

    private class Dependency3 : IDependency
    {
    }

    private class TriggeredDependencySource : IRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (service is IServiceWithType swt && swt.ServiceType == typeof(ITrigger))
            {
                yield return RegistrationBuilder.ForType<Dependency2>().As<ITrigger>().As<IDependency>().CreateRegistration();
            }
        }
    }
}
