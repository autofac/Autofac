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
        public void PreparingRaisedForEachResolveInstancePerDependency()
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
        public void PreparingRaisedOnceSingleInstance()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .SingleInstance()
                .OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<object>();
            Assert.Equal(1, preparingRaised);
        }

        [Fact]
        public void PreparingRaisedForFirstResolveInEachLifetimeScope()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .InstancePerLifetimeScope()
                .OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<object>();
            Assert.Equal(1, preparingRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<object>();
                Assert.Equal(2, preparingRaised);
                innerScope.Resolve<object>();
                Assert.Equal(2, preparingRaised);
            }

            container.Resolve<object>();
            Assert.Equal(2, preparingRaised);
        }

        [Fact]
        public void ActivatingRaisedForEachResolveInstancePerDependency()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<object>();
            Assert.Equal(2, activatingRaised);
        }

        [Fact]
        public void ActivatingRaisedOnceSingleInstance()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .SingleInstance()
                .OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<object>();
            Assert.Equal(1, activatingRaised);
        }

        [Fact]
        public void ActivatingRaisedForFirstResolveInEachLifetimeScope()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .InstancePerLifetimeScope()
                .OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<object>();
            Assert.Equal(1, activatingRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<object>();
                Assert.Equal(2, activatingRaised);
                innerScope.Resolve<object>();
                Assert.Equal(2, activatingRaised);
            }

            container.Resolve<object>();
            Assert.Equal(2, activatingRaised);
        }

        [Fact]
        public void ActivatedRaisedForEachResolveInstancePerDependency()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<object>();
            Assert.Equal(2, activatedRaised);
        }

        [Fact]
        public void ActivatedRaisedOnceSingleInstance()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .SingleInstance()
                .OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<object>();
            Assert.Equal(1, activatedRaised);
        }

        [Fact]
        public void ActivatedRaisedForFirstResolveInEachLifetimeScope()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .InstancePerLifetimeScope()
                .OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<object>();
            Assert.Equal(1, activatedRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<object>();
                Assert.Equal(2, activatedRaised);
                innerScope.Resolve<object>();
                Assert.Equal(2, activatedRaised);
            }

            container.Resolve<object>();
            Assert.Equal(2, activatedRaised);
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
        public void OnReleaseForSingletonStillFiresIfNotResolved()
        {
            var builder = new ContainerBuilder();

            var instance = new ReleasingClass();

            builder.RegisterInstance(instance)
                   .OnRelease(s => s.Release());

            using (var container = builder.Build())
            {
                using (var scope = container.BeginLifetimeScope())
                {
                }
            }

            Assert.True(instance.Released);
        }

        private class ReleasingClass
        {
            public bool Released { get; set; }

            public void Release()
            {
                Released = true;
            }
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
