using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Metadata;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class LifetimeEventTests
    {
        [Fact]
        public void PreparingCanProvideParametersToActivator()
        {
            IEnumerable<Parameter> parameters = new Parameter[] { new NamedParameter("n", 1) };
            IEnumerable<Parameter> actual = null;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .OnPreparing(e => e.Parameters = parameters)
                .OnActivating(e => actual = e.Parameters);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.False(parameters.Except(actual).Any());
        }

        [Fact]
        public void PreparingRaisedForEachResolve()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<object>();
            Assert.Equal(2, preparingRaised);
        }

        [Fact]
        public void RegisteredCanModifyRegistrations()
        {
            var marker = "marker";
            var builder = new ContainerBuilder();
            builder
                .RegisterType<object>()
                .OnRegistered(e =>
                {
                    e.ComponentRegistration.Metadata[marker] = marker;
                });

            var container = builder.Build();

            var obj = container.Resolve<Meta<object>>();
            Assert.Equal(marker, obj.Metadata[marker]);
        }

        [Fact]
        public void RegisteredRaisedOnContainerBuild()
        {
            var registeredRaised = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().OnRegistered(e => registeredRaised++);
            var container = builder.Build();
            Assert.Equal(1, registeredRaised);
        }
    }
}
