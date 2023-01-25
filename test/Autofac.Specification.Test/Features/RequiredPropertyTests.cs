using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Tag { get; }
    }

    private class ServiceB
    {
        public ServiceB()
        {
            Tag = "Default";
        }

        public string Tag { get; }
    }

}

#endif
