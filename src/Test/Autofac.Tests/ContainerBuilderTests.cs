using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Features.Indexed;

#if !WINDOWS_PHONE
using Moq;
#endif

using NUnit.Framework;
using Autofac.Core;
using System.Linq;

namespace Autofac.Tests
{
    // This fixture is in desperate need of some love.
    // Ideally all of the different kinds of registration and syntax extension should be
    // tested in their own fixtures.
    [TestFixture]
    public class ContainerBuilderTests
    {
        interface IA { }
        interface IB { }
        interface IC { }

        public class Abc : DisposeTracker, IA, IB, IC { }

        [Test]
        public void SimpleReg()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>();
            var c = cb.Build();
            var a = c.Resolve<Abc>();
            Assert.IsNotNull(a);
            Assert.IsInstanceOf<Abc>(a);
        }

        [Test]
        public void SimpleRegIface()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>().As<IA>();
            var c = cb.Build();
            var a = c.Resolve<IA>();
            Assert.IsNotNull(a);
            Assert.IsInstanceOf<Abc>(a);
            Assert.IsFalse(c.IsRegistered<Abc>());
        }

        [Test]
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

            Assert.IsNotNull(a1);
            Assert.AreNotSame(a1, 12);
            Assert.IsFalse(((Abc)a1).IsDisposed);
            Assert.IsFalse(((Abc)a2).IsDisposed);
        }

        [Test]
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

            Assert.IsNotNull(a1);
            Assert.AreSame(a1, a2);
            Assert.IsTrue(((Abc)a1).IsDisposed);
            Assert.IsTrue(((Abc)a2).IsDisposed);
        }

        [Test]
        public void WithFactoryContext()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Abc>().As<IA>();
            var c = cb.Build();
            var ctx = c.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = ctx.Resolve<IA>();
            ctx.Dispose();

            Assert.IsNotNull(a1);
            Assert.AreNotSame(a1, a2);
            Assert.IsTrue(((Abc)a1).IsDisposed);
            Assert.IsTrue(((Abc)a2).IsDisposed);
        }

        [Test]
        public void RegistrationOrderingPreserved()
        {
            var target = new ContainerBuilder();
            var inst1 = new object();
            var inst2 = new object();
            target.RegisterInstance(inst1);
            target.RegisterInstance(inst2);
            Assert.AreSame(inst2, target.Build().Resolve<object>());
        }

        class ObjectModule : Module
        {
            public bool ConfigureCalled { get; private set; }

            #region IModule Members

            protected override void Load(ContainerBuilder builder)
            {
                if (builder == null) throw new ArgumentNullException("builder");
                ConfigureCalled = true;
                builder.RegisterType<object>().SingleInstance();
            }

            #endregion
        }

        [Test]
        public void RegisterModule()
        {
            var mod = new ObjectModule();
            var target = new ContainerBuilder();
            target.RegisterModule(mod);
            Assert.IsFalse(mod.ConfigureCalled);
            var container = target.Build();
            Assert.IsTrue(mod.ConfigureCalled);
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OnlyAllowBuildOnce()
        {
            var target = new ContainerBuilder();
            target.Build();
            target.Build();
        }

        public class A1 { }
        public class A2 { }

        public class Named
        {
            public delegate Named Factory(string name);

            public string Name { get; set; }

            public Named(string name, object o)
            {
                Name = name;
            }
        }

        [Test]
        public void RegisterWithName()
        {
            var name = "object.registration";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(name);

            var c = cb.Build();

            object o1;
            Assert.IsTrue(c.TryResolveNamed(name, typeof(object), out o1));
            Assert.IsNotNull(o1);

            object o2;
            Assert.IsFalse(c.TryResolve(typeof(object), out o2));
        }

        [Test]
        public void RegisterWithKey()
        {
            var key = new object();

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Keyed<object>( key );

            var c = cb.Build();

            object o1;
            Assert.IsTrue(c.TryResolveKeyed(key, typeof(object), out o1));
            Assert.IsNotNull(o1);

            object o2;
            Assert.IsFalse(c.TryResolve(typeof(object), out o2));
        }

        [Test]
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
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out registration));

            Assert.AreEqual(2, registration.Metadata.Count);
            Assert.IsTrue(registration.Metadata.Contains(p1));
            Assert.IsTrue(registration.Metadata.Contains(p2));
        }

        [Test]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnPreparing(e => ++preparingFired);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.AreEqual(1, preparingFired);
        }

        [Test]
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

        class Module1 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<object>();
            }
        }

        class Module2 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterModule(new Module1());
            }
        }

        [Test]
        public void ModuleCanRegisterModule()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new Module2());
            var container = builder.Build();

            container.AssertRegistered<object>();
        }

        [Test]
        public void RegisterTypeAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Test]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA, IB>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Test]
        public void RegisterInstanceAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Test]
        public void RegisterAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => "hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Test]
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
            Assert.IsNotNull(a);
            Assert.AreSame(a, b);
            Assert.AreSame(b, c);
        }

        [Test]
        public void DefaultsPreservedWhenSpecified()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            var container = builder.Build();
            Assert.AreEqual("s1", container.Resolve<string>());
        }

        [Test]
        public void WhenNoDefaultNewRegistrationIsAvailable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s2").PreserveExistingDefaults();
            var container = builder.Build();
            Assert.AreEqual("s2", container.Resolve<string>());
        }

        [Test, Ignore("#272")]
        public void DefaultsPreservedInNestedScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("s1");
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => 
                b.RegisterInstance("s2").PreserveExistingDefaults());

            Assert.AreEqual("s1", scope.Resolve<string>());
        }

        [Test]
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

        [Test]
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

            Assert.AreSame(container.ComponentRegistry, registry);
            Assert.AreSame(marker, cr.Metadata[marker]);
        }

        static void AssertIsContainerScoped<TSvc>(IComponentContext ctx1, IComponentContext ctx2)
        {
            Assert.AreSame(ctx1.Resolve<TSvc>(), ctx1.Resolve<TSvc>());
            Assert.AreNotSame(ctx1.Resolve<TSvc>(), ctx2.Resolve<TSvc>());
        }

        [Test]
        public void ProvidedInstancesCannotSupportInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void WhenUpdating_DefaultModulesAreExcluded()
        {
            var builder = new ContainerBuilder();
            var container = new Container();
            builder.Update(container);
            Assert.IsFalse(container.IsRegistered<IEnumerable<object>>());
        }

        [Test]
        public void WhenBuildingWithDefaultsExcluded_DefaultModulesAreExcluded()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);
            Assert.IsFalse(container.IsRegistered<IEnumerable<object>>());
        }

        [Test]
        public void WhenTIsRegisteredByKey_IndexCanRetrieveIt()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, string>>();
            Assert.AreSame(cpt, idx[key]);
        }

        [Test]
        public void WhenTIsRegisteredByKey_IndexComposesWithIEnumerableOfT()
        {
            var key = 42;
            var cpt = "Hello";
            var builder = new ContainerBuilder();
            builder.RegisterInstance(cpt).Keyed<string>(key);
            var container = builder.Build();

            var idx = container.Resolve<IIndex<int, IEnumerable<string>>>();
            Assert.AreSame(cpt, idx[key].Single());
        }

        [Test]
        public void AfterCallingBuild_SubsequentCallsFail()
        {
            var builder = new ContainerBuilder();
            var c = builder.Build();

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.That(ex.Message.Contains("once"));
        }

#if !WINDOWS_PHONE
        [Test]
        public void WhenTheContainerIsBuilt_StartableComponentsAreStarted()
        {
            const ContainerBuildOptions buildOptions = ContainerBuildOptions.Default;
            var started = WasStartInvoked(buildOptions);
            Assert.IsTrue(started);
        }

        [Test]
        public void WhenNoStartIsSpecified_StartableComponentsAreIgnored()
        {
            const ContainerBuildOptions buildOptions = ContainerBuildOptions.IgnoreStartableComponents;
            var started = WasStartInvoked(buildOptions);
            Assert.IsFalse(started);
        }

        static bool WasStartInvoked(ContainerBuildOptions buildOptions)
        {
            var started = false;
            var startable = new Mock<IStartable>();
            startable.Setup(s => s.Start())
                .Callback(() => started = true);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable.Object);
            builder.Build(buildOptions);
            return started;
        }
#endif
    }
}
