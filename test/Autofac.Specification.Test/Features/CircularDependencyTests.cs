using System;
using Autofac.Core;
using Autofac.Specification.Test.Features.CircularDependency;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class CircularDependencyTests
    {
        [Fact]
        public void DetectsCircularDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<D>().As<ID>();
            builder.RegisterType<A>().As<IA>();
            builder.RegisterType<BC>().As<IB, IC>();

            var container = builder.Build();

            var de = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ID>());
        }
    }
}
