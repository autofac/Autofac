using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public void ActivatedAllowsTaskReturningHandler()
        {
            var pval = 12;
            var builder = new ContainerBuilder();
            builder.RegisterType<MethodInjection>()
                .InstancePerLifetimeScope()
                .OnActivated(async e =>
                {
                    await Task.Delay(1);
                    e.Instance.Method(pval);
                });

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
        public void ChainedOnActivatingEventsAreInvokedWithinASingleResolveOperation()
        {
            var builder = new ContainerBuilder();

            var secondEventRaised = false;
            builder.RegisterType<object>()
                .Named<object>("second")
                .OnActivating(e => secondEventRaised = true);

            builder.RegisterType<object>()
                .OnActivating(e => e.Context.ResolveNamed<object>("second"));

            var container = builder.Build();
            container.Resolve<object>();

            Assert.True(secondEventRaised);
        }

        [Fact]
        public void MultipleOnActivatingEventsCanPassReplacementOnward()
        {
            var builder = new ContainerBuilder();

            var activatingInstances = new List<object>();

            builder.RegisterType<AService>()
                .OnActivating(e =>
                {
                    activatingInstances.Add(e.Instance);
                    e.ReplaceInstance(new AServiceChild());
                })
                .OnActivating(e => activatingInstances.Add(e.Instance));

            var container = builder.Build();
            var result = container.Resolve<AService>();

            Assert.IsType<AServiceChild>(result);
            Assert.Equal(2, activatingInstances.Count);
            Assert.IsType<AService>(activatingInstances[0]);
            Assert.IsType<AServiceChild>(activatingInstances[1]);
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
            cb.RegisterType<AService>()
                .OnPreparing(e => e.Parameters = parameters)
                .OnActivating(e => actual = e.Parameters);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.False(parameters.Except(actual).Any());
        }

        [Fact]
        public void AsyncPreparingCanProvideParametersToActivator()
        {
            IEnumerable<Parameter> parameters = new Parameter[] { new NamedParameter("n", 1) };
            IEnumerable<Parameter> actual = null;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .OnPreparing(async e =>
                {
                    await Task.Delay(1);
                    e.Parameters = parameters;
                })
                .OnActivating(e => actual = e.Parameters);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.False(parameters.Except(actual).Any());
        }

        [Fact]
        public void PreparingRaisedForEachResolveInstancePerDependency()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>().OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<AService>();
            Assert.Equal(2, preparingRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(3, preparingRaised);
            }
        }

        [Fact]
        public void PreparingRaisedOnceSingleInstance()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .SingleInstance()
                .OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<AService>();
            Assert.Equal(1, preparingRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(1, preparingRaised);
            }
        }

        [Fact]
        public void PreparingRaisedForFirstResolveInEachLifetimeScope()
        {
            var preparingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .InstancePerLifetimeScope()
                .OnPreparing(e => preparingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, preparingRaised);
            container.Resolve<AService>();
            Assert.Equal(1, preparingRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<AService>();
                Assert.Equal(2, preparingRaised);
                innerScope.Resolve<AService>();
                Assert.Equal(2, preparingRaised);
            }

            container.Resolve<AService>();
            Assert.Equal(2, preparingRaised);
        }

        [Fact]
        public void PreparingOnlyRaisedForAttachedRegistrations()
        {
            var preparingRaised = new List<IComponentRegistration>();
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .As<IService>()
                .OnPreparing(e => preparingRaised.Add(e.Component));
            cb.RegisterType<BService>()
                .As<IService>();
            var container = cb.Build();
            container.Resolve<IEnumerable<IService>>();
            Assert.Single(preparingRaised);
            Assert.Equal(typeof(AService), preparingRaised[0].Activator.LimitType);
        }

        [Fact]
        public void ActivatingRaisedForEachResolveInstancePerDependency()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>().OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<AService>();
            Assert.Equal(2, activatingRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(3, activatingRaised);
            }
        }

        [Fact]
        public void ActivatingRaisedOnceSingleInstance()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .SingleInstance()
                .OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(1, activatingRaised);
            }
        }

        [Fact]
        public void AsyncActivatingSupported()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .OnActivating(async e =>
                {
                    await Task.Delay(1);
                    activatingRaised++;
                });
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
        }

        [Fact]
        public void ActivatingRaisedForFirstResolveInEachLifetimeScope()
        {
            var activatingRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .InstancePerLifetimeScope()
                .OnActivating(e => activatingRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
            container.Resolve<AService>();
            Assert.Equal(1, activatingRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<AService>();
                Assert.Equal(2, activatingRaised);
                innerScope.Resolve<AService>();
                Assert.Equal(2, activatingRaised);
            }

            container.Resolve<AService>();
            Assert.Equal(2, activatingRaised);
        }

        [Fact]
        public void ActivatingOnlyRaisedForAttachedRegistrations()
        {
            var activatingRaised = new List<IComponentRegistration>();
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .As<IService>()
                .OnActivating(e =>
                {
                    activatingRaised.Add(e.Component);
                });
            cb.RegisterType<BService>()
                .As<IService>();
            var container = cb.Build();
            container.Resolve<IEnumerable<IService>>();
            Assert.Single(activatingRaised);
            Assert.Equal(typeof(AService), activatingRaised[0].Activator.LimitType);
        }

        [Fact]
        public void ActivatedRaisedForEachResolveInstancePerDependency()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>().OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<AService>();
            Assert.Equal(2, activatedRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(3, activatedRaised);
            }
        }

        [Fact]
        public void ActivatedRaisedOnceSingleInstance()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .SingleInstance()
                .OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<AService>();
            Assert.Equal(1, activatedRaised);
            using (var inner = container.BeginLifetimeScope())
            {
                container.Resolve<AService>();
                Assert.Equal(1, activatedRaised);
            }
        }

        [Fact]
        public void ActivatedRaisedForFirstResolveInEachLifetimeScope()
        {
            var activatedRaised = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .InstancePerLifetimeScope()
                .OnActivated(e => activatedRaised++);
            var container = cb.Build();
            container.Resolve<AService>();
            Assert.Equal(1, activatedRaised);
            container.Resolve<AService>();
            Assert.Equal(1, activatedRaised);
            using (var innerScope = container.BeginLifetimeScope())
            {
                innerScope.Resolve<AService>();
                Assert.Equal(2, activatedRaised);
                innerScope.Resolve<AService>();
                Assert.Equal(2, activatedRaised);
            }

            container.Resolve<AService>();
            Assert.Equal(2, activatedRaised);
        }

        [Fact]
        public void ActivatedOnlyRaisedForAttachedRegistrations()
        {
            var activatedRaised = new List<IComponentRegistration>();
            var activatedInstances = new List<IService>();
            var cb = new ContainerBuilder();
            cb.RegisterType<AService>()
                .As<IService>();
            cb.RegisterType<BService>()
                .As<IService>()
                .OnActivated(e =>
                {
                    activatedRaised.Add(e.Component);
                    activatedInstances.Add(e.Instance);
                });
            var container = cb.Build();
            container.Resolve<IEnumerable<IService>>();
            Assert.Single(activatedRaised);
            Assert.Equal(typeof(BService), activatedRaised[0].Activator.LimitType);
            Assert.Single(activatedInstances);
            Assert.IsType<BService>(activatedInstances[0]);
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
        public void AsyncReleaseHandlersRunUnderNormalDisposal()
        {
            var builder = new ContainerBuilder();
            object instance = null;
            builder.RegisterType<DisposeTracker>()
                .OnRelease(async i =>
                {
                    await Task.Delay(1);
                    instance = i;
                });

            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.Same(dt, instance);
        }

        [Fact]
        public async Task AsyncReleaseHandlersRunUnderAsyncDisposal()
        {
            var asyncLocal = new AsyncLocal<int>();
            asyncLocal.Value = 5;

            var builder = new ContainerBuilder();
            object instance = null;
            builder.RegisterType<DisposeTracker>()
                .OnRelease(async i =>
                {
                    await Task.Delay(1);

                    // Assert that the async local is preserved.
                    Assert.Equal(5, asyncLocal.Value);
                    instance = i;
                });

            var container = builder.Build();
            var dt = container.Resolve<DisposeTracker>();

            await container.DisposeAsync();

            Assert.Same(dt, instance);
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

        [Fact]
        public void OnReleaseForSingletonAsInterfaceStillFiresIfNotResolved()
        {
            var builder = new ContainerBuilder();

            var instance = new ReleasingClass();

            builder.RegisterInstance(instance)
                   .As<IReleasingService>()
                   .OnRelease(s => s.Release());

            using (var container = builder.Build())
            {
                using (var scope = container.BeginLifetimeScope())
                {
                }
            }

            Assert.True(instance.Released);
        }

        private interface IService
        {
        }

        private class AService : IService
        {
        }

        private class AServiceChild : AService
        {
        }

        private class BService : IService
        {
        }

        private class ReleasingClass : IReleasingService
        {
            public bool Released { get; set; }

            public void Release()
            {
                Released = true;
            }
        }

        private interface IReleasingService
        {
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
