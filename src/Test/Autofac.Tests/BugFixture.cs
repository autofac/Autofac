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
                builder.RegisterType<D>().As<ID>();
                builder.RegisterType<A>().As<IA>();
                builder.RegisterType<BC>().As<IB, IC>();

                var container = builder.Build();

                ID d = container.Resolve<ID>();

                Assert.Fail("Expected circular dependency exception.");
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsTrue(de.Message.Contains(
                    "Autofac.Tests.BugFixture+D -> Autofac.Tests.BugFixture+A -> Autofac.Tests.BugFixture+BC -> Autofac.Tests.BugFixture+A"));
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
            var inner = container.BeginLifetimeScope();
            bool unused = inner.IsRegistered<string>();
        }

        [Test, Ignore("Needs attention")]
        public void UnresolvedProvidedInstancesNeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void MultipleServicesResultInMultipleRegistrationsBug()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named("a").Named("b").InstancePerLifetimeScope();
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            inner.Resolve("a");
            var count = inner.ComponentRegistry.Registrations.Count();
            inner.Resolve("b");
            Assert.AreEqual(count, inner.ComponentRegistry.Registrations.Count());
        }

        //[Test]
        //public void MultipleServicesResultInMultipleRegistrationsGeneric()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterGeneric(typeof(List<>)).As(typeof(IEnumerable<>), typeof(ICollection<>)).ContainerScoped().UsingConstructor();
        //    var container = builder.Build();
        //    var inner = container.BeginLifetimeScope();
        //    inner.Resolve<IEnumerable<int>>();
        //    var count = inner.ComponentRegistry.Registrations.Count();
        //    inner.Resolve<ICollection<int>>();
        //    Assert.AreEqual(count, inner.ComponentRegistry.Registrations.Count());
        //}
    }
}
