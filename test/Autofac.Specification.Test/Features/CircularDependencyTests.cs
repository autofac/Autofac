using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Specification.Test.Features.CircularDependency;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class CircularDependencyTests
    {
        private interface IPlugin
        {
        }

        private interface IService
        {
        }

        private interface IUnavailableComponent
        {
        }

        [Fact]
        public void ActivationStackResetsOnFailedLambdaResolve()
        {
            // Issue #929
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceImpl>().AsSelf();
            builder.Register<IService>(c =>
            {
                try
                {
                    // This will fail because ServiceImpl needs a Guid ctor
                    // parameter and it's not provided.
                    return c.Resolve<ServiceImpl>();
                }
                catch (Exception)
                {
                    // This is where the activation stack isn't getting reset.
                }

                return new ServiceImpl(Guid.Empty);
            });
            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterType<ComponentConsumer>().AsSelf();
            var container = builder.Build();

            // This throws a circular dependency exception if the activation stack
            // doesn't get reset.
            container.Resolve<ComponentConsumer>();
        }

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

        [Fact]
        public void InstancePerDependencyDoesNotAllowCircularDependencies_ConstructorOwnerResolved()
        {
            var cb = new ContainerBuilder();
            var ac = 0;
            cb.RegisterType<DependsByCtor>().OnActivating(e => { ac = 2; });
            cb.RegisterType<DependsByProp>().OnActivating(e => { ac = 1; })
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => c.Resolve<DependsByCtor>());

            Assert.Equal(2, ac);
        }

        [Fact]
        public void InstancePerDependencyDoesNotAllowCircularDependencies_PropertyOwnerResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>();
            cb.RegisterType<DependsByProp>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => c.Resolve<DependsByProp>());
        }

        [Fact]
        public void InstancePerLifetimeScopeServiceCannotCreateSecondInstanceOfSelfDuringConstruction()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AThatDependsOnB>().InstancePerLifetimeScope();
            builder.RegisterType<BThatCreatesA>().InstancePerLifetimeScope();
            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<AThatDependsOnB>());
        }

        [Fact]
        public void ManualEnumerableRegistrationDoesNotCauseCircularDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<RootViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PluginsViewModel>().AsSelf().SingleInstance();

            builder.RegisterType(typeof(Plugin1)).Named<IPlugin>(nameof(Plugin1));
            builder.RegisterType(typeof(Plugin2)).Named<IPlugin>(nameof(Plugin2));
            builder.Register(
                ctx => new[] { nameof(Plugin1), nameof(Plugin2) }
                    .Select(name => SafeResolvePlugin(name, ctx))
                    .Where(p => p != null)
                    .ToArray())
                .As<IEnumerable<IPlugin>>()
                .SingleInstance();

            var container = builder.Build();

            // From issue 648, this resolve call was getting a circular dependency
            // detection exception. It shouldn't be getting anything because the "safe resolve"
            // eats the dependency resolution issue for Plugin2 and Plugin1 should be
            // properly resolved.
            Assert.NotNull(container.Resolve<RootViewModel>());
            Assert.Single(container.Resolve<IEnumerable<IPlugin>>());
        }

        [Fact]
        public void SingleInstanceAllowsCircularDependencies_ConstructorOwnerResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.NotNull(dbc.Dep);
            Assert.NotNull(dbc.Dep.Dep);
            Assert.Same(dbc, dbc.Dep.Dep);
        }

        [Fact]
        public void SingleInstanceAllowsCircularDependencies_PropertyOwnerResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();

            Assert.NotNull(dbp.Dep);
            Assert.NotNull(dbp.Dep.Dep);
            Assert.Same(dbp, dbp.Dep.Dep);
        }

        [Fact]
        public void CircularDependenciesHandledWhenAllDependenciesPropertyInjected()
        {
            // Issue #1085 - StackOverflowException thrown with circular property dependencies.
            var builder = new ContainerBuilder();

            builder.RegisterType<CircularDependencyB>()
                .As<ICircularDependencyB>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterType<CircularDependencyA>()
                .As<ICircularDependencyA>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterType<CircularDependencyHost>()
                .As<ICircularDependencyHost>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var container = builder.Build();

            var host = container.Resolve<ICircularDependencyHost>();

            Assert.NotNull(host.DependencyA);
            Assert.NotNull(host.DependencyB);

            Assert.Same(host.DependencyA, host.DependencyB.DependencyA);
            Assert.Same(host.DependencyB, host.DependencyA.DependencyB);
        }

        private interface ICircularDependencyA
        {
            ICircularDependencyB DependencyB { get; }
        }

        private interface ICircularDependencyB
        {
            ICircularDependencyA DependencyA { get; }
        }

        private interface ICircularDependencyHost
        {
            public ICircularDependencyB DependencyB { get; }

            public ICircularDependencyA DependencyA { get; }
        }

        private class CircularDependencyA : ICircularDependencyA
        {
            public ICircularDependencyB DependencyB { get; set; }
        }

        private class CircularDependencyB : ICircularDependencyB
        {
            public ICircularDependencyA DependencyA { get; set; }
        }

        private class CircularDependencyHost : ICircularDependencyHost
        {
            public ICircularDependencyB DependencyB { get; set; }

            public ICircularDependencyA DependencyA { get; set; }
        }

        private static IPlugin SafeResolvePlugin(string pluginName, IComponentContext core)
        {
            try
            {
                // Plugin2 will get filtered out because it has an
                // unavailable dependency.
                return core.ResolveNamed<IPlugin>(pluginName);
            }
            catch (DependencyResolutionException)
            {
                return null;
            }
        }

        private class AThatDependsOnB
        {
            public AThatDependsOnB(BThatCreatesA bThatCreatesA)
            {
            }
        }

        private class BThatCreatesA
        {
            public BThatCreatesA(Func<BThatCreatesA, AThatDependsOnB> factory)
            {
                factory(this);
            }
        }

        // Issue #929
        // When a resolve operation fails in a lambda registration the activation stack
        // doesn't get reset and incorrectly causes a circular dependency exception.
        //
        // The ComponentConsumer takes IService (ServiceImpl) and a Dependency; the
        // Dependency also takes an IService. Normally this wouldn't cause an issue, but
        // if the registration for IService is a lambda that has a try/catch around a failing
        // resolve, the activation stack won't reset and the IService will be seen as a
        // circular dependency.
        private class ComponentConsumer
        {
            private Dependency _dependency;

            private IService _service;

            public ComponentConsumer(IService service, Dependency dependency)
            {
                this._service = service;
                this._dependency = dependency;
            }
        }

        private class Dependency
        {
            private IService _service;

            public Dependency(IService service)
            {
                this._service = service;
            }
        }

        private class Plugin1 : IPlugin
        {
        }

        private class Plugin2 : IPlugin
        {
            public Plugin2(IUnavailableComponent unavailableComponent)
            {
            }
        }

        private class PluginsViewModel
        {
            public PluginsViewModel(IEnumerable<IPlugin> plugins)
            {
            }
        }

        private class RootViewModel
        {
            public RootViewModel(IEnumerable<IPlugin> plugins, PluginsViewModel pluginsViewModel)
            {
            }
        }

        private class ServiceImpl : IService
        {
            private Guid _id;

            public ServiceImpl(Guid id)
            {
                this._id = id;
            }
        }
    }
}
