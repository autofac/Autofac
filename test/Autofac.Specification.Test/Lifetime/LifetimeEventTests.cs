using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class LifetimeEventTests
    {
        [Fact]
        public void ActivatedAllowsMethodInjection()
        {
            var pval = 12;
            var builder = new ContainerBuilder();
            builder.RegisterType<MethodInjection>()
                .InstancePerLifetimeScope()
                .OnActivated(e => e.Instance.Method(pval));
            var container = builder.Build();
            var scope = container.BeginLifetimeScope();
            var invokee = scope.Resolve<MethodInjection>();
            Assert.Equal(pval, invokee.Param);
        }

        [Fact]
        public void ActivatedCanReceiveParameters()
        {
            const int provided = 12;
            var passed = 0;

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .OnActivated(e => passed = e.Parameters.TypedAs<int>());
            var container = builder.Build();

            container.Resolve<object>(TypedParameter.From(provided));
            Assert.Equal(provided, passed);
        }

        [Fact]
        public void ActivatingCanReceiveParameters()
        {
            const int provided = 12;
            var passed = 0;

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .OnActivating(e => passed = e.Parameters.TypedAs<int>());
            var container = builder.Build();

            container.Resolve<object>(TypedParameter.From(provided));
            Assert.Equal(provided, passed);
        }

        [Fact]
        public void ChainedOnActivatedEventsAreInvokedWithinASingleResolveOperation()
        {
            var builder = new ContainerBuilder();

            var secondEventRaised = false;
            builder.RegisterType<object>()
                .Named<object>("second")
                .OnActivated(e => secondEventRaised = true);

            builder.RegisterType<object>()
                .OnActivated(e => e.Context.ResolveNamed<object>("second"));

            var container = builder.Build();
            container.Resolve<object>();

            Assert.True(secondEventRaised);
        }

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

        [Fact]
        public void ReleaseHandlersGetInstanceBeingReleased()
        {
            var builder = new ContainerBuilder();
            object instance = null;
            builder.RegisterType<DisposeTracker>()
                .OnRelease(i => { instance = i; });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.Same(dt, instance);
        }

        [Fact]
        public void ReleaseStopsAutomaticDisposal()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>()
                .OnRelease(_ => { });
            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.False(dt.IsDisposed);
        }

        [Fact]
        public void EventRaisedFromComponentRegistrationCanGetServiceBeingResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MethodInjection>()
                .OnPreparing((e) =>
                {
                    var service = e.Service as IServiceWithType;
                    Assert.Equal(typeof(MethodInjection), service.ServiceType);
                })
                .OnActivating((e) =>
                {
                    var service = e.Service as IServiceWithType;
                    Assert.Equal(typeof(MethodInjection), service.ServiceType);
                })
                .OnActivated((e) =>
                {
                    var service = e.Service as IServiceWithType;
                    Assert.Equal(typeof(MethodInjection), service.ServiceType);
                });
            builder.Build().Resolve<MethodInjection>();
        }

        private class MethodInjection
        {
            public int Param { get; private set; }

            public void Method(int param)
            {
                this.Param = param;
            }
        }
    }
}
