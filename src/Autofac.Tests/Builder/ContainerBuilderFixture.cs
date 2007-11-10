using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;
using Autofac.Component.Activation;
using Autofac.Component.Registration;

namespace Autofac.Tests.Builder
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
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInstanceNull()
		{
			var builder = new ContainerBuilder();
			builder.Register((object)null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterDelegateNull()
		{
			var target = new ContainerBuilder();
			target.Register((ComponentActivator<object>)null);
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

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterTypeAsUnsupportedService()
		{
			new ContainerBuilder().Register<object>().As<string>();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterTypeAsSupportedAndUnsupportedService()
		{
			new ContainerBuilder().Register<object>().As<object, string>();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterInstanceAsUnsupportedService()
		{
			new ContainerBuilder().Register(new object()).As<string>();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterDelegateAsUnsupportedService()
		{
			new ContainerBuilder().Register(c => new object()).As<string>();
		}

		[Test]
		public void RegisterThreeServices()
		{
			var target = new ContainerBuilder();
			target.Register<Abc>()
                .As<IA, IB, IC>()
                .WithScope(InstanceScope.Singleton);
			var container = target.Build();
			var a = container.Resolve<IA>();
			var b = container.Resolve<IB>();
			var c = container.Resolve<IC>();
			Assert.IsNotNull(a);
			Assert.AreSame(a, b);
			Assert.AreSame(b, c);
		}

		class ObjectModule : IModule
		{
			public bool ConfigureCalled { get; private set; }

			#region IModule Members

			public void Configure(Container container)
			{
				if (container == null) throw new ArgumentNullException("container");
				ConfigureCalled = true;
				container.RegisterComponent(new ComponentRegistration(
					new[] { typeof(object) },
					new ProvidedInstanceActivator(new object())));
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

		[Test]
		public void BuildGenericRegistration()
		{
			var cb = new ContainerBuilder();
			cb.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>)).WithScope(InstanceScope.Factory);
			var c = cb.Build();

			ICollection<int> coll = c.Resolve<ICollection<int>>();
			ICollection<int> coll2 = c.Resolve<ICollection<int>>();

			Assert.IsNotNull(coll);
			Assert.IsNotNull(coll2);
			Assert.AreNotSame(coll, coll2);
			Assert.IsTrue(coll.GetType().GetGenericTypeDefinition() == typeof(List<>));
		}

        class Parameterised
        {
            public string A { get; private set; }
            public int B { get; private set; }

            public Parameterised(string a, int b)
            {
                A = a;
                B = b;
            }
        }

        [Test]
        public void RegisterParameterisedWithDelegate()
        {
            var cb = new ContainerBuilder();
            cb.Register((c, p) => new Parameterised(p.Get<string>("a"), p.Get<int>("b")));
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new NamedValue("a", aVal),
                new NamedValue("b", bVal));
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }

        [Test]
        public void RegisterParameterisedWithReflection()
        {
            var cb = new ContainerBuilder();
            cb.Register<Parameterised>();
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new NamedValue("a", aVal),
                new NamedValue("b", bVal));
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }
        class HasSetter
        {
            string _val;

            public string Val
            {
                get
                {
                    return _val;
                }
                set
                {
                    _val = value;
                }
            }
        }

        [Test]
        public void SetterInjection()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasSetter>()
                .OnActivating(ActivationHandler.InjectProperties);
            
            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
        }

        class HasSetterWithValue
        {
            string _val = "Default";

            public string Val
            {
                get
                {
                    return _val;
                }
                set
                {
                    _val = value;
                }
            }
        }

        [Test]
        public void SetterInjectionWithValue()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasSetterWithValue>()
                .OnActivating(ActivationHandler.InjectUnsetProperties);
            
            var container = builder.Build();
            
            var instance = container.Resolve<HasSetterWithValue>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
        }

        class HasPropReadOnly
        {
            string _val = "Default";

            public string Val
            {
                get
                {
                    return _val;
                }
                protected set
                {
                    _val = value;
                }
            }
        }

        [Test]
        public void SetterInjectionReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasPropReadOnly>()
                .OnActivating(ActivationHandler.InjectProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
        }

        [Test]
        public void SetterInjectionUnsetReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasPropReadOnly>()
                .OnActivating(ActivationHandler.InjectUnsetProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
        }

        class HasPropWriteOnly
        {
            string _val;

            public string Val
            {
                set
                {
                    _val = value;
                }
            }

            public string GetVal()
            {
                return _val;
            }
        }

        [Test]
        public void SetterInjectionWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasPropWriteOnly>()
                .OnActivating(ActivationHandler.InjectProperties);

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.GetVal());
        }

        [Test]
        public void SetterInjectionUnsetWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasPropWriteOnly>()
                .OnActivating(ActivationHandler.InjectUnsetProperties);

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.GetVal());
        }

        class A1 { }
        class A2 { }

        class TwoCtors
        {
            public Type[] CalledCtor { get; private set; }

            public TwoCtors(A1 a1)
            {
                CalledCtor = new[] { typeof(A1) };
            }

            public TwoCtors(A1 a1, A2 a2)
            {
                CalledCtor = new[] { typeof(A1), typeof(A2) };
            }
        }

        [Test]
        public void ExplicitCtorCalled()
        {
            var cb = new ContainerBuilder();
            cb.Register<A1>();
            cb.Register<A2>();

            var selected = new[]{ typeof(A1), typeof(A2) };

            cb.Register<TwoCtors>()
                .UsingConstructor(selected);

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(TwoCtors).GetConstructor(selected),
                typeof(TwoCtors).GetConstructor(result.CalledCtor));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitCtorNotPresent()
        {
            var cb = new ContainerBuilder();
            cb.Register<TwoCtors>()
                .UsingConstructor(typeof(A2));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExplicitCtorNull()
        {
            var cb = new ContainerBuilder();
            cb.Register<TwoCtors>()
                .UsingConstructor(null);
        }

        class WithParam
        {
            public int I { get; private set; }
            public WithParam(int i) { I = i; }
        }

        [Test]
        public void ParametersProvided()
        {
            var ival = 10;

            var cb = new ContainerBuilder();
            cb.Register<WithParam>().
                WithArguments(new NamedValue("i", ival));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();
            Assert.IsNotNull(result);
            Assert.AreEqual(ival, result.I);
        }

        class WithProp
        {
            public string Prop { get; set; }
        }

        [Test]
        public void PropertyProvided()
        {
            var pval = "Hello";

            var cb = new ContainerBuilder();
            cb.Register<WithProp>()
                .WithProperties(new NamedValue("Prop", pval));

            var c = cb.Build();

            var result = c.Resolve<WithProp>();
            Assert.IsNotNull(result);
            Assert.AreEqual(pval, result.Prop);
        }

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
        [Ignore("Investigate: Instance is never resolved thus not added to the disposer.")]
        public void DefaultOwnershipChanged()
        {
            var contextOwnerBuilder = new ContainerBuilder();
            using (contextOwnerBuilder.SetDefaultOwnership(InstanceOwnership.Container))
            {
                var disposable = new DisposeTracker();
                contextOwnerBuilder.Register(disposable);
                contextOwnerBuilder.Build().Dispose();
                Assert.IsTrue(disposable.IsDisposed);
            }

            var nonOwnedBuilder = new ContainerBuilder();
            using (nonOwnedBuilder.SetDefaultOwnership(InstanceOwnership.External))
            {
                var notDisposed = new DisposeTracker();
                nonOwnedBuilder.Register(notDisposed);
                nonOwnedBuilder.Build().Dispose();
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
        public void RegisterFactory()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>();
            cb.RegisterFactory<Named.Factory>((c, p) =>
                new Named(
                    p.Get<string>("name"),
                    c.Resolve<object>()));

            var container = cb.Build();
            Named.Factory factory = container.Resolve<Named.Factory>();
            Assert.IsNotNull(factory);

            string name = "Fred";
            var fred = factory.Invoke(name);
            Assert.IsNotNull(fred);
            Assert.AreEqual(name, fred.Name);
        }

        [Test]
        public void RegisterThroughFactory()
        {
            var cb = new ContainerBuilder();

            cb.Register<object>();
            cb.Register<Named>().ThroughFactory<Named.Factory>();

            var container = cb.Build();
       
            Named.Factory factory = container.Resolve<Named.Factory>();
            
            Assert.IsNotNull(factory);
            Assert.IsFalse(container.IsRegistered<Named>());
            
            string name = "Fred";
            var fred = factory.Invoke(name);
            Assert.IsNotNull(fred);
            Assert.AreEqual(name, fred.Name);
        }
    }
}
