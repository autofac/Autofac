// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Test.Features.Decorators;

public class DecoratorTests
{
    private interface IDecoratedService : IService
    {
        IDecoratedService Decorated
        {
            get;
        }
    }

    private interface IService
    {
    }

    private interface IAutoWiredService
    {
        bool NestedServiceIsNotNull();
    }

    private class NestedService
    {
    }

    private class AutoWiredService : IAutoWiredService
    {
        public NestedService NestedService
        {
            get; set;
        }

        public bool NestedServiceIsNotNull()
        {
            return NestedService is not null;
        }
    }

    private class AutoWiredServiceDecorator : IAutoWiredService
    {
        private readonly IAutoWiredService _original;

        public AutoWiredServiceDecorator(IAutoWiredService original)
        {
            _original = original;
        }

        public bool NestedServiceIsNotNull()
        {
            return _original.NestedServiceIsNotNull();
        }
    }

    private class Foo
    {
    }

    private class Bar : IBar
    {
    }

    private interface IBar
    {
    }

    private class BarDecorator : IBar
    {
        public BarDecorator(IBar bar)
        {
        }
    }

    [Fact]
    public void DecoratorWorksOnAdapter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Foo>();
        builder.RegisterAdapter<Foo, IBar>(f => new Bar());
        builder.RegisterDecorator<BarDecorator, IBar>();
        var container = builder.Build();
        Assert.IsType<BarDecorator>(container.Resolve<IBar>());
    }

    [Fact]
    public void DecoratedInstancePerDependencyRegistrationCanIncludeOtherServices()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ImplementorA>().As<IDecoratedService>().As<IService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var container = builder.Build();

        var serviceRegistration = container.RegistrationFor<IService>();
        var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService>();

        Assert.NotNull(serviceRegistration);
        Assert.NotNull(decoratedServiceRegistration);
        Assert.Same(serviceRegistration, decoratedServiceRegistration);

        var serviceInstance = container.Resolve<IService>();
        Assert.IsType<ImplementorA>(serviceInstance);

        var decoratedServiceInstance = container.Resolve<IDecoratedService>();
        Assert.IsType<DecoratorA>(decoratedServiceInstance);

        Assert.NotSame(serviceInstance, decoratedServiceInstance);
    }

    [Fact]
    public void DecoratedInstancePerLifetimeScopeRegistrationCanIncludeOtherServices()
    {
        var builder = new ContainerBuilder();

        // #963: The InstancePerLifetimeScope here is important - a single component may expose multiple services.
        // If that component is decorated, the decorator ALSO needs to expose all of those services.
        builder.RegisterType<ImplementorA>().As<IDecoratedService>().As<IService>().InstancePerLifetimeScope();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var container = builder.Build();

        var serviceRegistration = container.RegistrationFor<IService>();
        var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService>();

        Assert.NotNull(serviceRegistration);
        Assert.NotNull(decoratedServiceRegistration);
        Assert.Same(serviceRegistration, decoratedServiceRegistration);

        var serviceInstance = container.Resolve<IService>();
        Assert.IsType<ImplementorA>(serviceInstance);

        var decoratedServiceInstance = container.Resolve<IDecoratedService>();
        Assert.IsType<DecoratorA>(decoratedServiceInstance);

        Assert.Same(serviceInstance, decoratedServiceInstance.Decorated);
    }

    [Fact]
    public void DecoratedSingleInstanceRegistrationCanIncludeOtherServices()
    {
        var builder = new ContainerBuilder();

        // #963: The SingleInstance here is important - a single component may expose multiple services.
        // If that component is decorated, the decorator ALSO needs to expose all of those services.
        builder.RegisterType<ImplementorA>().As<IDecoratedService>().As<IService>().SingleInstance();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var container = builder.Build();

        var serviceRegistration = container.RegistrationFor<IService>();
        var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService>();

        Assert.NotNull(serviceRegistration);
        Assert.NotNull(decoratedServiceRegistration);
        Assert.Same(serviceRegistration, decoratedServiceRegistration);

        var serviceInstance = container.Resolve<IService>();
        Assert.IsType<ImplementorA>(serviceInstance);

        var decoratedServiceInstance = container.Resolve<IDecoratedService>();
        Assert.IsType<DecoratorA>(decoratedServiceInstance);

        Assert.Same(serviceInstance, decoratedServiceInstance.Decorated);
    }

    [Fact]
    public void RegistrationIncludesTheServiceType()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ImplementorA>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var container = builder.Build();

        var registration = container.RegistrationFor<IDecoratedService>();
        Assert.NotNull(registration);

        var decoratedService = new TypedService(typeof(IDecoratedService));
        Assert.Contains(registration.Services.OfType<TypedService>(), s => s == decoratedService);
    }

    [Fact]
    public void RegistrationTargetsTheImplementationType()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ImplementorA>().As<IDecoratedService>();
        builder.RegisterDecorator<DecoratorA, IDecoratedService>();
        var container = builder.Build();

        var registration = container.RegistrationFor<IDecoratedService>();

        Assert.NotNull(registration);
        Assert.Equal(typeof(ImplementorA), registration.Target.Activator.LimitType);
    }

    [Fact]
    public void DecorateReflectionActivatorWithPropertyInjection()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<NestedService>();
        builder.RegisterType<AutoWiredService>().As<IAutoWiredService>().PropertiesAutowired();
        builder.RegisterDecorator<AutoWiredServiceDecorator, IAutoWiredService>();

        var container = builder.Build();

        var service = container.Resolve<IAutoWiredService>();

        Assert.True(service.NestedServiceIsNotNull());
    }

    [Fact]
    public void DecorateProvidedInstanceActivatorWithPropertyInjection()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<NestedService>();
        builder.RegisterInstance<IAutoWiredService>(new AutoWiredService()).PropertiesAutowired();
        builder.RegisterDecorator<AutoWiredServiceDecorator, IAutoWiredService>();

        var container = builder.Build();

        var service = container.Resolve<IAutoWiredService>();

        Assert.True(service.NestedServiceIsNotNull());
    }

    // Issue 1459: A decorator constructor may take a dependency typed as a more
    // derived service than the one being decorated. The decorated instance must
    // not be force-injected into that parameter; it should be resolved normally.
    private interface IBase
    {
    }

    private interface IDerived : IBase
    {
    }

    private class BaseImpl : IBase
    {
    }

    private class DerivedImpl : IDerived
    {
    }

    private record DerivedDependencyDecorator(IDerived Derived, IBase Decorated) : IBase;

    [Fact]
    public void DecoratorWithMoreDerivedServiceDependencyResolvesDependencyNormally()
    {
        // Issue 1459: The decorated service (IBase) should be supplied to the
        // "Decorated" parameter, while the more-derived "Derived" (IDerived)
        // parameter must be resolved from the container rather than receiving
        // the decorated IBase instance (which is not an IDerived).
        var builder = new ContainerBuilder();
        builder.RegisterType<BaseImpl>().As<IBase>();
        builder.RegisterType<DerivedImpl>().As<IDerived>();
        builder.RegisterDecorator<DerivedDependencyDecorator, IBase>();

        var container = builder.Build();

        var resolved = container.Resolve<IBase>();

        var decorator = Assert.IsType<DerivedDependencyDecorator>(resolved);
        Assert.IsType<BaseImpl>(decorator.Decorated);
        Assert.IsType<DerivedImpl>(decorator.Derived);
    }

    private abstract class Decorator : IDecoratedService
    {
        protected Decorator(IDecoratedService decorated)
        {
            Decorated = decorated;
        }

        public IDecoratedService Decorated
        {
            get;
        }
    }

    private class DecoratorA : Decorator
    {
        public DecoratorA(IDecoratedService decorated)
            : base(decorated)
        {
        }
    }

    private class ImplementorA : IDecoratedService
    {
        public IDecoratedService Decorated => this;
    }
}
