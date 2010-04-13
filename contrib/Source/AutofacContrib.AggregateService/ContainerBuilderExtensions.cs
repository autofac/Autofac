using System;
using Autofac;

namespace AutofacContrib.AggregateService
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterAggregateService<TInterface>(this ContainerBuilder builder) where TInterface:class 
        {
            builder.RegisterAggregateService(typeof (TInterface));
        }

        public static void RegisterAggregateService (this ContainerBuilder builder, Type aggregateServiceInterfaceType)
        {
            builder.Register(c => AggregateServiceGenerator.CreateInstance(aggregateServiceInterfaceType, c.Resolve<IComponentContext>()))
                .As(aggregateServiceInterfaceType)
                .InstancePerLifetimeScope();
        }
    }
}