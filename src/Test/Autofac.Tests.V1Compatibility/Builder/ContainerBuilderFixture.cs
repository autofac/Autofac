using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using NUnit.Framework;

namespace Autofac.Tests.V1Compatibility.Builder
{
	[TestFixture]
	public class ContainerBuilderFixture
	{
		interface IA { }
		interface IB { }
		interface IC { }

		class Abc : DisposeTracker, IA, IB, IC { }

		[Test]
		public void SimpleReg()
		{
			var cb = new ContainerBuilder();
			cb.Register<Abc>();
			var c = cb.Build();
			var a = c.Resolve<Abc>();
			Assert.IsNotNull(a);
			Assert.IsInstanceOfType(typeof(Abc), a);
		}

		[Test]
		public void SimpleRegIface()
		{
			var cb = new ContainerBuilder();
			cb.Register<Abc>().As<IA>();
			var c = cb.Build();
			var a = c.Resolve<IA>();
			Assert.IsNotNull(a);
			Assert.IsInstanceOfType(typeof(Abc), a);
			Assert.IsFalse(c.IsRegistered<Abc>());
		}

		[Test]
		public void WithExternalFactory()
		{
			var cb = new ContainerBuilder();
			cb.Register<Abc>()
				.As<IA>()
				.WithOwnership(InstanceOwnership.External)
				.WithScope(InstanceScope.Factory);
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
			cb.Register<Abc>()
				.As<IA>()
				.WithOwnership(InstanceOwnership.Container)
				.WithScope(InstanceScope.Singleton);
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
			cb.Register<Abc>()
				.As<IA>()
				.WithOwnership(InstanceOwnership.Container)
				.WithScope(InstanceScope.Factory);
			var c = cb.Build();
			var ctx = c.CreateInnerContainer();
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
			target.Register(inst1);
			target.Register(inst2);
			Assert.AreSame(inst2, target.Build().Resolve<object>());
		}

		class ObjectModule : IModule
		{
			public bool ConfigureCalled { get; private set; }

			#region IModule Members

			public void Configure(IContainer container)
			{
				if (container == null) throw new ArgumentNullException("container");
				ConfigureCalled = true;
				container.RegisterComponent(
                    new Registration(
                        new Descriptor(
                            new UniqueService(), 
					        new Service[] { new TypedService(typeof(object)) },
                            typeof(object)),
				        new ProvidedInstanceActivator(new object()),
				        new SingletonScope(),
				        InstanceOwnership.Container));
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

        class A1 { }
        class A2 { }

        [Test]
        public void DefaultScopeChanged()
        {
            var builder = new ContainerBuilder();
            using (builder.SetDefaultScope(InstanceScope.Factory))
            {
                using (builder.SetDefaultScope(InstanceScope.Singleton))
                {
                    // Should have been changed to Singleton
                    builder.Register<A2>();
                }

                // Should revert to Factory
                builder.Register<A1>();
            }
            
            var container = builder.Build();
            Assert.AreNotSame(container.Resolve<A1>(), container.Resolve<A1>());
            Assert.AreSame(container.Resolve<A2>(), container.Resolve<A2>());
        }

        [Test]
        public void DefaultOwnershipChanged()
        {
            var contextOwnerBuilder = new ContainerBuilder();
            using (contextOwnerBuilder.SetDefaultOwnership(InstanceOwnership.Container))
            {
                var disposable = new DisposeTracker();
                contextOwnerBuilder.Register(disposable);
                var container = contextOwnerBuilder.Build();
                container.Resolve<DisposeTracker>();
                container.Dispose();
                Assert.IsTrue(disposable.IsDisposed);
            }

            var nonOwnedBuilder = new ContainerBuilder();
            using (nonOwnedBuilder.SetDefaultOwnership(InstanceOwnership.External))
            {
                var notDisposed = new DisposeTracker();
                nonOwnedBuilder.Register(notDisposed);
                var container = nonOwnedBuilder.Build();
                container.Resolve<DisposeTracker>();
                container.Dispose();
                Assert.IsFalse(notDisposed.IsDisposed);
            }
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

        [Test]
        public void RegisterWithName()
        {
            var name = "object.registration";

            var cb = new ContainerBuilder();
            cb.Register<object>().Named(name);

            var c = cb.Build();

            object o1;
            Assert.IsTrue(c.TryResolve(name, out o1));
            Assert.IsNotNull(o1);

            object o2;
            Assert.IsFalse(c.TryResolve(typeof(object), out o2));
        }

        [Test]
        public void WithExtendedProperties()
        {
            var p1 = new KeyValuePair<string, object>("p1", "p1Value");
            var p2 = new KeyValuePair<string, object>("p2", "p2Value");

            var builder = new ContainerBuilder();
            builder.Register<object>()
                .WithExtendedProperty(p1.Key, p1.Value)
                .WithExtendedProperty(p2.Key, p2.Value);

            var container = builder.Build();

            IComponentRegistration registration;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out registration));

            Assert.AreEqual(2, registration.Descriptor.ExtendedProperties.Count);
            Assert.IsTrue(registration.Descriptor.ExtendedProperties.Contains(p1));
            Assert.IsTrue(registration.Descriptor.ExtendedProperties.Contains(p2));
        }

        [Test]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.Register<object>().OnPreparing((s, e) => ++preparingFired);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.AreEqual(1, preparingFired);
        }

        class Module1 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.Register<object>();
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
    }
}
