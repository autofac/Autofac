using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests
{
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
                Assert.IsTrue(de.Message.Contains("Autofac.Tests.BugFixture+IA -> Autofac.Tests.BugFixture+IC -> Autofac.Tests.BugFixture+IA"));
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
        [Ignore("Investigate: Instance is never resolved thus not added to the disposer.")]
        public void UnresolvedProvidedInstancesNeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.Register(disposable);
            var container = builder.Build();
            container.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }
    }
}
