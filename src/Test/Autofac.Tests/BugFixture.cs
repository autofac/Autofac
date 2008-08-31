using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests
{
    /// <summary>
    /// This should be called the 'paranoia fixture' - suspicions get aired here.
    /// </summary>
    [TestFixture]
    public class BugFixture
    {
        interface IA { }

        interface IB { }

        interface IC { }

        interface ID { }

        class A : IA
        {
            public A(IC c) { }
        }

        class BC : IB, IC
        {
            public BC(IA a) { }
        }

        class D : ID
        {
            public D(IA a, IC c) { }
        }

        [Test]
        public void IncorrectExceptionThrown1()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.Register<D>().As<ID>();
				builder.Register<A>().As<IA>();
				builder.Register<BC>().As<IB, IC>();

				var container = builder.Build();

                ID d = container.Resolve<ID>();

                Assert.Fail("Expected circular dependency exception.");
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsTrue(de.Message.Contains(
                    "Autofac.Tests.BugFixture+ID -> Autofac.Tests.BugFixture+IA -> Autofac.Tests.BugFixture+IC -> Autofac.Tests.BugFixture+IA"));
            }
            catch (Exception ex)
            {
                Assert.Fail("Wrong exception type caught: " + ex.ToString());
            }
        }

        [Test]
        public void ServiceNotRegisteredExceptionOnIsRegistered()
        {
            var container = new Container();
            var inner = container.CreateInnerContainer();
            bool unused = inner.IsRegistered<string>();
        }

        [Test]
        public void UnresolvedProvidedInstancesNeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.Register(disposable);
            var container = builder.Build();
            container.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }
        
        [Test]
        public void ResolvingChangesDefaultInSubcontext()
        {
        	var a = new object();
        	var b = new object();
        	var builder = new ContainerBuilder();
            builder.SetDefaultScope(InstanceScope.Factory);
        	builder.Register(c => a).Named("a").Named("other");
            builder.Register(c => b).Named("b").Named("other");
        	var container = builder.Build();
        	var outerOther = container.Resolve("other");
        	Assert.AreSame(b, outerOther);
        	var inner = container.CreateInnerContainer();
        	var innerOtherOne = inner.Resolve("other");
        	Assert.AreSame(b, innerOtherOne);
        	inner.Resolve("a");
        	var innerOtherTwo = inner.Resolve("other");
        	Assert.AreSame(b, innerOtherTwo);
        }

        [Test]
        public void ResolvingChangesDefaultInSubcontext2()
        {
            var a = new object();
            var b = new object();
            var builder = new ContainerBuilder();
            builder.SetDefaultScope(InstanceScope.Factory);
            builder.Register(c => a).Named("a").Named("other");
            builder.Register(c => b).Named("b").Named("other");
            var container = builder.Build();
            var outerOther = container.Resolve("other");
            Assert.AreSame(b, outerOther);
            var inner = container.CreateInnerContainer();
            inner.Resolve("a");
            var innerOther = inner.Resolve("other");
            Assert.AreSame(b, innerOther);
        }

        [Test]
        public void ResolvingChangesDefaultInSubcontext3()
        {
            var builder = new ContainerBuilder();
            builder.SetDefaultScope(InstanceScope.Factory);
            builder.Register<object>().Named("a").Named("other");
            builder.Register<object>().Named("b").Named("other");
            var container = builder.Build();
            var inner = container.CreateInnerContainer();
            IComponentRegistration cr1, cr2;
            inner.TryGetDefaultRegistrationFor(new NamedService("other"), out cr1);
            inner.Resolve("a");
            inner.TryGetDefaultRegistrationFor(new NamedService("other"), out cr2);
            Assert.AreSame(cr1, cr2);
        }

        [Test]
        public void RegisteringChangesDefaultInSubcontext()
        {
            var builder = new ContainerBuilder();
            builder.SetDefaultScope(InstanceScope.Factory);
            builder.Register<object>().Named("a").Named("other");
            var container = builder.Build();
            var inner = container.CreateInnerContainer();
            IComponentRegistration cr1, cr2;
            inner.Resolve("a");
            inner.TryGetDefaultRegistrationFor(new NamedService("other"), out cr1);
            var innerBuilder = new ContainerBuilder();
            innerBuilder.Register<object>().Named("other");
            innerBuilder.Build(inner);
            inner.TryGetDefaultRegistrationFor(new NamedService("other"), out cr2);
            Assert.AreNotSame(cr1, cr2);
        }

        [Test]
        public void MultipleServicesResultInMultipleRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.Register<object>().Named("a").Named("b").ContainerScoped();
            var container = builder.Build();
            var inner = container.CreateInnerContainer();
            inner.Resolve("a");
            var count = inner.ComponentRegistrations.Count();
            inner.Resolve("b");
            Assert.AreEqual(count, inner.ComponentRegistrations.Count());
        }

        [Test]
        public void IsRegisteredCreatesRegistration()
        {
            AssertCreatesRegistration((c, svc) => c.IsRegistered(svc));
        }

        [Test]
        public void TryGetDefaultRegistrationForCreatesRegistration()
        {
            IComponentRegistration unused;
            AssertCreatesRegistration((c, svc) => c.TryGetDefaultRegistrationFor(svc, out unused));
        }

        [Test]
        public void ResolveCreatesRegistration()
        {
            AssertCreatesRegistration((c, svc) => c.Resolve(svc));
        }

        void AssertCreatesRegistration(Action<IContainer, Service> action)
        {
            var builder = new ContainerBuilder();
            builder.Register<object>().ContainerScoped();
            var outer = builder.Build();
            var inner = outer.CreateInnerContainer();
            var count = inner.ComponentRegistrations.Count();
            action(inner, new TypedService(typeof(object)));
            Assert.IsTrue(count < inner.ComponentRegistrations.Count());
        }

        [Test]
        public void MultipleServicesResultInMultipleRegistrationsGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(List<>)).As(typeof(IEnumerable<>), typeof(ICollection<>)).ContainerScoped().UsingConstructor();
            var container = builder.Build();
            var inner = container.CreateInnerContainer();
            inner.Resolve<IEnumerable<int>>();
            var count = inner.ComponentRegistrations.Count();
            inner.Resolve<ICollection<int>>();
            Assert.AreEqual(count, inner.ComponentRegistrations.Count());
        }
        
        [Test]
        public void ResolvingChangesDefaultInSubcontext3Generic()
        {
            var builder = new ContainerBuilder();
            builder.SetDefaultScope(InstanceScope.Factory);
            builder.RegisterGeneric(typeof(List<>)).As(typeof(IList<>), typeof(ICollection<>)).UsingConstructor();
            builder.RegisterGeneric(typeof(List<>)).As(typeof(IEnumerable<>), typeof(ICollection<>)).UsingConstructor();
            var container = builder.Build();
            var inner = container.CreateInnerContainer();
            IComponentRegistration cr1, cr2;
            inner.TryGetDefaultRegistrationFor(new TypedService(typeof(ICollection<int>)), out cr1);
            inner.Resolve<IList<int>>();
            inner.TryGetDefaultRegistrationFor(new TypedService(typeof(ICollection<int>)), out cr2);
            Assert.AreSame(cr1, cr2);
        }
    }
}
