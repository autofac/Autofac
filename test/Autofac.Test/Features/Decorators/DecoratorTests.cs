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

    private class DerivedDependencyDecorator : IBase
    {
        public DerivedDependencyDecorator(IDerived derived, IBase decorated)
        {
            Derived = derived;
            Decorated = decorated;
        }

        public IDerived Derived
        {
            get;
        }

        public IBase Decorated
        {
            get;
        }
    }

    private class BaseDecorator : IBase
    {
        public BaseDecorator(IBase decorated)
        {
            Decorated = decorated;
        }

        public IBase Decorated
        {
            get;
        }
    }

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

    [Fact]
    public void DecoratorWithMoreDerivedServiceDependencyResolvesDependencyNormallyInChain()
    {
        // Issue 1459: When decorators are chained, the decorated instance seen by
        // the outer decorator is the inner decorator's output (via
        // DecoratorContext.UpdateContext), which is an IBase but not an IDerived.
        // The outer decorator's more-derived "Derived" (IDerived) parameter must
        // still be resolved from the container rather than receiving that chained
        // instance, while "Decorated" receives the inner decorator.
        var builder = new ContainerBuilder();
        builder.RegisterType<BaseImpl>().As<IBase>();
        builder.RegisterType<DerivedImpl>().As<IDerived>();

        // Registered first => innermost decorator.
        builder.RegisterDecorator<BaseDecorator, IBase>();
        builder.RegisterDecorator<DerivedDependencyDecorator, IBase>();

        var container = builder.Build();

        var resolved = container.Resolve<IBase>();

        var outer = Assert.IsType<DerivedDependencyDecorator>(resolved);
        Assert.IsType<DerivedImpl>(outer.Derived);

        var inner = Assert.IsType<BaseDecorator>(outer.Decorated);
        Assert.IsType<BaseImpl>(inner.Decorated);
    }

    [Fact]
    public void DecoratorWithUnregisteredMoreDerivedServiceDependencyThrowsResolutionException()
    {
        // Issue 1459: When the more-derived "Derived" (IDerived) parameter is not
        // registered, the parameter falls through to normal autowiring, which
        // cannot satisfy it. The result should be a clean DependencyResolutionException
        // rather than the previous InvalidCastException from force-injecting the
        // decorated instance.
        var builder = new ContainerBuilder();
        builder.RegisterType<BaseImpl>().As<IBase>();

        // IDerived is intentionally not registered.
        builder.RegisterDecorator<DerivedDependencyDecorator, IBase>();

        var container = builder.Build();

        Assert.Throws<DependencyResolutionException>(() => container.Resolve<IBase>());
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
