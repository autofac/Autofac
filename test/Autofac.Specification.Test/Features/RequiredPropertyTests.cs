// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Specification.Test.Features;

public class RequiredPropertyTests
{
    [Fact]
    public void ResolveRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<Component>();

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.NotNull(component.ServiceA);
        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void MissingRequiredPropertyServiceThrowsException()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>();

        var container = builder.Build();

        var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<Component>());

        Assert.Contains(nameof(Component.ServiceB), exception?.InnerException?.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CanMixConstructorsAndProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<MixedConstructorAndPropertyComponent>();

        var container = builder.Build();

        var component = container.Resolve<MixedConstructorAndPropertyComponent>();

        Assert.NotNull(component.ServiceA);
        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void PropertiesPopulatedOnAllSubsequentResolves()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<Component>();

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.NotNull(component.ServiceA);
        Assert.NotNull(component.ServiceB);

        var component2 = container.Resolve<Component>();

        Assert.NotNull(component2.ServiceA);
        Assert.NotNull(component2.ServiceB);

        var component3 = container.Resolve<Component>();

        Assert.NotNull(component3.ServiceA);
        Assert.NotNull(component3.ServiceB);
    }

    [Fact]
    public void ExplicitParameterOverridesRequiredAutowiring()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<Component>().WithProperty(nameof(Component.ServiceB), new ServiceB { Tag = "custom" });

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.Equal("custom", component.ServiceB.Tag);
    }

    [Fact]
    public void ExplicitPropertyAppliedOnAllSubsequentResolves()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithProperty(nameof(Component.ServiceB), new ServiceB { Tag = "custom" });

        var container = builder.Build();

        for (var i = 0; i < 3; i++)
        {
            var component = container.Resolve<Component>();

            Assert.NotNull(component.ServiceA);
            Assert.Equal("custom", component.ServiceB.Tag);
        }
    }

    [Fact]
    public void AutowiringParameterSubclassIsAskedForRequiredProperty()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<Component>();

        var container = builder.Build();

        var custom = new ServiceB { Tag = "custom" };
        var component = container.Resolve<Component>(new SuppliesServiceBAutowiringParameter(custom));

        Assert.Same(custom, component.ServiceB);
        Assert.Equal("Default", component.ServiceA.Tag);
    }

    [Fact]
    public void ExplicitParameterCanTakePlaceOfRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithProperty(nameof(Component.ServiceB), new ServiceB { Tag = "custom" });

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.Equal("custom", component.ServiceB.Tag);
    }

    [Fact]
    public void GeneralTypeParameterCanTakePlaceOfRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithParameter(new TypedParameter(typeof(ServiceB), new ServiceB()));

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void ParameterPassedAtResolveUsedForRequiredProperty()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>();

        var container = builder.Build();

        var component = container.Resolve<Component>(new TypedParameter(typeof(ServiceB), new ServiceB()));

        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void NamedParametersIgnoredForRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithParameter("value", new ServiceB());

        var container = builder.Build();

        Assert.Throws<DependencyResolutionException>(() => container.Resolve<Component>());
    }

    [Fact]
    public void PositionalParametersIgnoredForRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithParameter(new PositionalParameter(0, new ServiceB()));

        var container = builder.Build();

        Assert.Throws<DependencyResolutionException>(() => container.Resolve<Component>());
    }

    [Fact]
    public void SetsRequiredMembersConstructorSkipsRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ConstructorComponent>();

        var container = builder.Build();

        var component = container.Resolve<ConstructorComponent>();

        Assert.Null(component.ServiceA);
        Assert.Null(component.ServiceB);
    }

    [Fact]
    public void SetsRequiredMembersConstructorWithArgumentsSkipsRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<ArgumentConstructorComponent>();

        var container = builder.Build();

        var component = container.Resolve<ArgumentConstructorComponent>();

        Assert.Null(component.ServiceA);
        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void SetsRequiredMembersConstructorSkipsRequiredPropertiesEvenWhenRegistered()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<ConstructorComponent>();

        var container = builder.Build();

        var component = container.Resolve<ConstructorComponent>();

        Assert.Null(component.ServiceA);
        Assert.Null(component.ServiceB);
    }

    [Fact]
    public void OnlySelectedConstructorConsideredForSetsRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<MultiConstructorComponent>();

        var container = builder.Build();

        var component = container.Resolve<MultiConstructorComponent>();

        // Allowed to not be set, because empty constructor was selected.
        Assert.Null(component.ServiceA);

        using var scope = container.BeginLifetimeScope(b => b.RegisterType<ServiceC>());

        // Fails, because constructor with SetsRequiredProperties attribute is selected.
        Assert.Throws<DependencyResolutionException>(() => scope.Resolve<MultiConstructorComponent>());
    }

    [Fact]
    public void DerivedClassResolvesPropertiesInParent()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<DerivedComponent>();

        var container = builder.Build();

        var component = container.Resolve<DerivedComponent>();

        Assert.NotNull(component.ServiceA);
        Assert.NotNull(component.ServiceB);
    }

    [Fact]
    public void DerivedClassResolvesPropertiesInParentAndSelf()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<ServiceB>();
        builder.RegisterType<ServiceC>();
        builder.RegisterType<DerivedComponentWithProp>();

        var container = builder.Build();

        var component = container.Resolve<DerivedComponentWithProp>();

        Assert.NotNull(component.ServiceA);
        Assert.NotNull(component.ServiceB);
        Assert.NotNull(component.ServiceC);
    }

    [Fact]
    public void CanResolveOpenGenericComponentRequiredProperties()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(OpenGenericService<>));
        builder.RegisterGeneric(typeof(OpenGenericComponent<>));

        var container = builder.Build();

        var component = container.Resolve<OpenGenericComponent<int>>();

        Assert.NotNull(component.Service);
    }

    private class OpenGenericComponent<T>
    {
        public required OpenGenericService<T> Service
        {
            get; set;
        }
    }

    private class OpenGenericService<T>
    {
    }

    private class ConstructorComponent
    {
        [SetsRequiredMembers]
        public ConstructorComponent()
        {
        }

        public required ServiceA? ServiceA
        {
            get; set;
        }

        public required ServiceB? ServiceB
        {
            get; set;
        }
    }

    private class ArgumentConstructorComponent
    {
        [SetsRequiredMembers]
        public ArgumentConstructorComponent(ServiceB serviceB)
        {
            ServiceB = serviceB;
        }

        public required ServiceA? ServiceA
        {
            get; set;
        }

        public required ServiceB? ServiceB
        {
            get; set;
        }
    }

    private class MixedConstructorAndPropertyComponent
    {
        public MixedConstructorAndPropertyComponent(ServiceA serviceA)
        {
            ServiceA = serviceA;
        }

        public ServiceA ServiceA
        {
            get; set;
        }

        public required ServiceB ServiceB
        {
            get; set;
        }
    }

    private class MultiConstructorComponent
    {
        [SetsRequiredMembers]
        public MultiConstructorComponent()
        {
        }

        public MultiConstructorComponent(ServiceC serviceC)
        {
        }

        public required ServiceA? ServiceA
        {
            get; set;
        }
    }

    private class Component
    {
        public required ServiceA ServiceA
        {
            get; set;
        }

        public required ServiceB ServiceB
        {
            get; set;
        }
    }

    private class DerivedComponentWithProp : Component
    {
        public DerivedComponentWithProp()
        {
        }

        public required ServiceC ServiceC
        {
            get; set;
        }
    }

    private class DerivedComponent : Component
    {
    }

    private class ServiceA
    {
        public ServiceA()
        {
            Tag = "Default";
        }

        public string Tag
        {
            get; set;
        }
    }

    private class ServiceB
    {
        public ServiceB()
        {
            Tag = "Default";
        }

        public string Tag
        {
            get; set;
        }
    }

    private class ServiceC
    {
    }

    private class SuppliesServiceBAutowiringParameter : AutowiringParameter
    {
        private readonly ServiceB _value;

        public SuppliesServiceBAutowiringParameter(ServiceB value)
        {
            _value = value;
        }

        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(true)] out Func<object?>? valueProvider)
        {
            if (pi.ParameterType == typeof(ServiceB))
            {
                valueProvider = () => _value;
                return true;
            }

            return base.CanSupplyValue(pi, context, out valueProvider);
        }
    }
}
