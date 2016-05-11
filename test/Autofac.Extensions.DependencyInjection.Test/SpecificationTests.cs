using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class SpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var builder = new ContainerBuilder();

            builder.Populate(serviceCollection);

            IContainer container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }
    }
}
