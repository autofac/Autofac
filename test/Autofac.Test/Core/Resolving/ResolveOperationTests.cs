using System;
using System.Linq;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveOperationTests
    {
        [Fact]
        public void ActivatedArgsSuppliesParameters()
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
        public void ActivatingArgsSuppliesParameters()
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
        public void AfterTheOperationIsFinished_ReusingTheTemporaryContextThrows()
        {
            IComponentContext ctx = null;
            var builder = new ContainerBuilder();
            builder.Register(c =>
            {
                ctx = c;
                return new object();
            });
            builder.RegisterInstance("Hello");
            var container = builder.Build();
            container.Resolve<string>();
            container.Resolve<object>();
            Assert.Throws<ObjectDisposedException>(() => ctx.Resolve<string>());
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
        public void CtorPropDependencyFactoriesOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>();
            cb.RegisterType<DependsByProp>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => c.Resolve<DependsByProp>());
        }

        [Fact]
        public void CtorPropDependencyFactoriesOrder2()
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
        public void CtorPropDependencyOkOrder1()
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
        public void CtorPropDependencyOkOrder2()
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
        public void PropertyInjectionPassesNamedParameterOfTheInstanceTypeBeingInjectedOnto()
        {
            var capturedparameters = Enumerable.Empty<Parameter>();

            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired();
            cb.Register((context, parameters) =>
            {
                capturedparameters = parameters.ToArray();
                return new DependsByCtor(null);
            });

            var c = cb.Build();
            var existingInstance = new DependsByProp();
            c.InjectUnsetProperties(existingInstance);

            var instanceType = capturedparameters.Named<Type>(ResolutionExtensions.PropertyInjectedInstanceTypeNamedParameter);
            Assert.Equal(existingInstance.GetType(), instanceType);
        }

        private interface IService
        {
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
