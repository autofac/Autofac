using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;

namespace Autofac.Specification.Test.Features;

#if NET7_0_OR_GREATER

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

        Assert.Contains(nameof(Component.ServiceB), exception.InnerException.Message);
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
    public void GeneralTypePropertyParameterCanTakePlaceOfRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ServiceA>();
        builder.RegisterType<Component>().WithParameter(new TypedParameter(typeof(ServiceB), new ServiceB()));

        var container = builder.Build();

        var component = container.Resolve<Component>();

        Assert.NotNull(component.ServiceB);
    }

    private class Component
    {
        required public ServiceA ServiceA { get; set; }

        required public ServiceB ServiceB { get; set; }
    }

    private class ServiceA
    {
        public ServiceA()
        {
            Tag = "Default";
        }

        public string Tag { get; set; }
    }

    private class ServiceB
    {
        public ServiceB()
        {
            Tag = "Default";
        }

        public string Tag { get; set; }
    }

}

#endif
