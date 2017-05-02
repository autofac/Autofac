using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Indexed;
using Xunit;

namespace Autofac.Test
{
    // This fixture is in desperate need of some love.
    // Ideally all of the different kinds of registration and syntax extension should be
    // tested in their own fixtures.
    public class ContainerBuilderTests
    {
        internal interface IA
        {
        }

        internal interface IB
        {
        }

        internal interface IC
        {
        }

        public class Abc : DisposeTracker, IA, IB, IC
        {
        }

        [Fact]
        public void SimpleReg()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>();
            var c = cb.Build();
            var a = c.Resolve<Abc>();
            Assert.NotNull(a);
            Assert.IsType<Abc>(a);
        }

        [Fact]
        public void SimpleRegIface()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>().As<IA>();
            var c = cb.Build();
            var a = c.Resolve<IA>();
            Assert.NotNull(a);
            Assert.IsType<Abc>(a);
            Assert.False(c.IsRegistered<Abc>());
        }

        [Fact]
        public void WithExternalFactory()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>()
                .As<IA>()
                .ExternallyOwned();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var a2 = c.Resolve<IA>();
            c.Dispose();

            Assert.NotNull(a1);
            Assert.NotSame(a1, 12);
            Assert.False(((Abc)a1).IsDisposed);
            Assert.False(((Abc)a2).IsDisposed);
        }

        [Fact]
        public void WithInternalSingleton()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>()
                .As<IA>()
                .OwnedByLifetimeScope()
                .SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var a2 = c.Resolve<IA>();
            c.Dispose();

            Assert.NotNull(a1);
            Assert.Same(a1, a2);
            Assert.True(((Abc)a1).IsDisposed);
            Assert.True(((Abc)a2).IsDisposed);
        }

        [Fact]
        public void WithFactoryContext()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>().As<IA>();
            var c = cb.Build();
            var ctx = c.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = ctx.Resolve<IA>();
            ctx.Dispose();

            Assert.NotNull(a1);
            Assert.NotSame(a1, a2);
            Assert.True(((Abc)a1).IsDisposed);
            Assert.True(((Abc)a2).IsDisposed);
        }

        [Fact]
        public void RegistrationOrderingPreserved()
        {
            var target = new ContainerBuilder();
            var inst1 = new object();
            var inst2 = new object();
            target.RegisterInstance(inst1);
            target.RegisterInstance(inst2);
            Assert.Same(inst2, target.Build().Resolve<object>());
        }

        [Fact]
        public void OnlyAllowBuildOnce()
        {
            var target = new ContainerBuilder();
            target.Build();
            Assert.Throws<InvalidOperationException>(() => target.Build());
        }

        public class A1
        {
        }

        public class A2
        {
        }

        public class Named
        {
            public delegate Named Factory(string name);

            public string Name { get; set; }

            public Named(string name, object o)
            {
                Name = name;
            }
        }

        [Fact]
        public void RegisterWithName()
        {
            var name = "object.registration";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(name);

            var c = cb.Build();

            object o1;
            Assert.True(c.TryResolveNamed(name, typeof(object), out o1));
            Assert.NotNull(o1);

            object o2;
            Assert.False(c.TryResolve(typeof(object), out o2));
        }

        [Fact]
        public void RegisterWithKey()
        {
            var key = new object();

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Keyed<object>(key);

            var c = cb.Build();

            object o1;
            Assert.True(c.TryResolveKeyed(key, typeof(object), out o1));
            Assert.NotNull(o1);

            object o2;
            Assert.False(c.TryResolve(typeof(object), out o2));
        }

        [Fact]
        public void WithMetadata()
        {
            var p1 = new KeyValuePair<string, object>("p1", "p1Value");
            var p2 = new KeyValuePair<string, object>("p2", "p2Value");

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .WithMetadata(p1.Key, p1.Value)
                .WithMetadata(p2.Key, p2.Value);

            var container = builder.Build();

            IComponentRegistration registration;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out registration));

            Assert.True(registration.Metadata.Contains(p1));
            Assert.True(registration.Metadata.Contains(p2));
        }

        [Fact]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnPreparing(e => ++preparingFired);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.Equal(1, preparingFired);
        }

        [Fact]
        public void WhenPreparingHandlerProvidesParameters_ParamsProvidedToActivator()
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

        internal class Module1 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<object>();
            }
        }

        internal class Module2 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterModule(new Module1());
            }
        }

        [Fact]
        public void ModuleCanRegisterModule()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new Module2());
            var container = builder.Build();

            container.AssertRegistered<object>();
        }

        [Fact]
        public void RegisterTypeAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA, IB>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterInstanceAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => "hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterThreeServices()
        {
            var target = new ContainerBuilder();
            target.RegisterType<Abc>()
                .As<IA, IB, IC>()
                .SingleInstance();
            var container = target.Build();
            var a = container.Resolve<IA>();
            var b = container.Resolve<IB>();
            var c = container.Resolve<IC>();
            Assert.NotNull(a);
            Assert.Same(a, b);
            Assert.Same(b, c);
        }

        [Fact]
        public void InContextSpecifiesContainerScope()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope(contextName);
            var ctx2 = container.BeginLifetimeScope(contextName);

            AssertIsContainerScoped<object>(ctx1, ctx2);
        }

        [Fact]
        public void WhenContainerIsBuilt_OnRegisteredHandlersAreInvoked()
        {
            var builder = new ContainerBuilder();

            var marker = "marker";

            IComponentRegistry registry = null;
            IComponentRegistration cr = null;
            builder.RegisterType<object>()
                .WithMetadata(marker, marker)
                .OnRegistered(e =>
                {
                    registry = e.ComponentRegistry;
                    cr = e.ComponentRegistration;
                });

            var container = builder.Build();

            Assert.Same(container.ComponentRegistry, registry);
            Assert.Same(marker, cr.Metadata[marker]);
        }

        private static void AssertIsContainerScoped<TSvc>(IComponentContext ctx1, IComponentContext ctx2)
        {
            Assert.Same(ctx1.Resolve<TSvc>(), ctx1.Resolve<TSvc>());
            Assert.NotSame(ctx1.Resolve<TSvc>(), ctx2.Resolve<TSvc>());
        }

        [Fact]
        public void ProvidedInstancesCannotSupportInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void WhenUpdating_DefaultModulesAreExcluded()
        {
            var builder = new ContainerBuilder();
            var container = new Container();
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618
            Assert.False(container.IsRegistered<IEnumerable<object>>());
        }

        [Fact]
        public void WhenBuildingWithDefaultsExcluded_DefaultModulesAreExcluded()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);
            Assert.False(container.IsRegistered<IEnumerable<object>>());
        }

        [Fact]
        public void WhenTIsRegisteredByKey_IndexCanRetrieveIt()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, string>>();
            Assert.Same(cpt, idx[key]);
        }

        [Fact]
        public void WhenTIsRegisteredByKey_IndexComposesWithIEnumerableOfT()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, IEnumerable<string>>>();
            Assert.Same(cpt, idx[key].Single());
        }

        [Fact]
        public void AfterCallingBuild_SubsequentCallsFail()
        {
            var builder = new ContainerBuilder();
            var c = builder.Build();

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.True(ex.Message.Contains("once"));
        }

        [Fact(Skip = "Issue #722")]
        public void StartableComponentsObeySingletonUsage()
        {
            // Issue #722
            var builder = new ContainerBuilder();
            StartableDependency.Count = 0;
            builder.RegisterType<StartableTakesDependency>().AsImplementedInterfaces();
            builder.RegisterType<StartableDependency>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<StartableTakesDependency>().AsImplementedInterfaces();
            builder.RegisterType<StartableDependency>().AsImplementedInterfaces().SingleInstance();
            builder.Build();
            Assert.Equal(1, StartableDependency.Count);
        }

        [Fact]
        public void WhenTheContainerIsBuilt_StartableComponentsAreStarted()
        {
            const ContainerBuildOptions buildOptions = ContainerBuildOptions.None;
            var started = WasStartInvoked(buildOptions);
            Assert.True(started);
        }

        [Fact]
        public void WhenTheContainerIsUpdated_ExistingStartableComponentsAreNotReStarted()
        {
            var startable1 = Mocks.GetStartable();
            var startable2 = Mocks.GetStartable();

            var builder1 = new ContainerBuilder();
            builder1.RegisterInstance(startable1).As<IStartable>();
            var container = builder1.Build();

            Assert.Equal(1, startable1.StartCount);

            var builder2 = new ContainerBuilder();
            builder2.RegisterInstance(startable2).As<IStartable>();
#pragma warning disable CS0618
            builder2.Update(container);
#pragma warning restore CS0618

            Assert.Equal(1, startable1.StartCount);
            Assert.Equal(1, startable2.StartCount);
        }

        [Fact]
        public void WhenTheContainerIsUpdated_NewStartableComponentsAreStarted()
        {
            // Issue #454: ContainerBuilder.Update() doesn't activate startable components.
            var container = new ContainerBuilder().Build();

            var startable = Mocks.GetStartable();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618

            Assert.Equal(1, startable.StartCount);
        }

        [Fact]
        public void WhenNoStartIsSpecified_StartableComponentsAreIgnored()
        {
            const ContainerBuildOptions buildOptions = ContainerBuildOptions.IgnoreStartableComponents;
            var started = WasStartInvoked(buildOptions);
            Assert.False(started);
        }

        private static bool WasStartInvoked(ContainerBuildOptions buildOptions)
        {
            var startable = Mocks.GetStartable();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
            builder.Build(buildOptions);
            return startable.StartCount > 0;
        }

        private interface IStartableDependency
        {
        }

        private class StartableDependency : IStartableDependency
        {
            private static int _count = 0;

            public StartableDependency()
            {
                _count++;
            }

            public static int Count
            {
                get
                {
                    return _count;
                }

                set
                {
                    _count = value;
                }
            }
        }

        private class StartableTakesDependency : IStartable
        {
            public StartableTakesDependency(IStartableDependency[] dependencies)
            {
            }

            public void Start()
            {
            }
        }

        [Fact]
        public void CtorCreatesDefaultPropertyBag()
        {
            var builder = new ContainerBuilder();
            Assert.NotNull(builder.Properties);
        }

        [Fact]
        public void RegistrationsCanUsePropertyBag()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;
            builder.Register(ctx =>
            {
                // TOTALLY not thread-safe, but illustrates the point.
                var count = (int)ctx.ComponentRegistry.Properties["count"];
                count++;
                ctx.ComponentRegistry.Properties["count"] = count;
                return "incremented";
            }).As<string>();
            var container = builder.Build();

            container.Resolve<string>();
            container.Resolve<string>();

            Assert.Equal(2, container.ComponentRegistry.Properties["count"]);
        }

        [Fact]
        public void RegisterBuildCallbackThrowsWhenProvidedNullCallback()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterBuildCallback(null));

            Assert.Equal("buildCallback", exception.ParamName);
        }

        [Fact]
        public void RegisterBuildCallbackReturnsBuilderInstance()
        {
            var builder = new ContainerBuilder();

            Assert.Same(builder, builder.RegisterBuildCallback(c => { }));
        }

        [Fact]
        public void BuildCallbacksInvokedWhenContainerBuilt()
        {
            var called = 0;

            void BuildCallback(IContainer c)
            {
                called++;
            }

            new ContainerBuilder()
                .RegisterBuildCallback(BuildCallback)
                .RegisterBuildCallback(BuildCallback)
                .Build();

            Assert.Equal(2, called);
        }

        [Fact]
        public void BuildCallbacksInvokedWhenRegisteredInModuleLoad()
        {
            var module = new BuildCallbackModule();

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            builder.Build();

            Assert.Equal(2, module.Called);
        }

        public class BuildCallbackModule : Module
        {
            public int Called { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                void BuildCallback(IContainer c)
                {
                    Called++;
                }

                builder.RegisterBuildCallback(BuildCallback)
                    .RegisterBuildCallback(BuildCallback);
            }
        }
    }
}
