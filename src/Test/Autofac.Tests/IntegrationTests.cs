using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;
using Autofac.Tests.Scenarios.Dependencies.Circularity;
using Autofac.Tests.Scenarios.Graph1;

namespace Autofac.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void CanCorrectlyBuildGraph1()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();
            builder.RegisterType<CD1>().As<IC1, ID1>().SingleInstance();
            builder.RegisterType<E1>().SingleInstance();
            builder.Register(ctr => new B1(ctr.Resolve<A1>()));

            var target = builder.Build();

            E1 e = target.Resolve<E1>();
            A1 a = target.Resolve<A1>();
            B1 b = target.Resolve<B1>();
            IC1 c = target.Resolve<IC1>();
            ID1 d = target.Resolve<ID1>();

            Assert.IsInstanceOf<CD1>(c);
            CD1 cd = (CD1)c;

            Assert.AreSame(a, b.A);
            Assert.AreSame(a, cd.A);
            Assert.AreNotSame(b, cd.B);
            Assert.AreSame(c, e.C);
            Assert.AreNotSame(b, e.B);
            Assert.AreNotSame(e.B, cd.B);
        }

        [Test]
        public void DetectsAndIdentifiesCircularDependencies()
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
                    "Autofac.Tests.Scenarios.Dependencies.Circularity.D -> Autofac.Tests.Scenarios.Dependencies.Circularity.A -> Autofac.Tests.Scenarios.Dependencies.Circularity.BC -> Autofac.Tests.Scenarios.Dependencies.Circularity.A"));
            }
            catch (Exception ex)
            {
                Assert.Fail("Wrong exception type caught: " + ex.ToString());
            }
        }

        [Test]
        public void UnresolvedProvidedInstances_DisposedWithLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void UnresolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable).ExternallyOwned();
            var container = builder.Build();
            container.Dispose();
            Assert.IsFalse(disposable.IsDisposed);
        }

        interface I1<T> { }
        interface I2<T> { }
        class C<T> : I1<T>, I2<T> { }

        [Test]
        public void MultipleServicesOnAnOpenGenericType_ShareTheSameRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(C<>)).As(typeof(I1<>), typeof(I2<>));
            var container = builder.Build();
            container.Resolve<I1<int>>();
            var count = container.ComponentRegistry.Registrations.Count();
            container.Resolve<I2<int>>();
            Assert.AreEqual(count, container.ComponentRegistry.Registrations.Count());
        }

        [Test]
        public void ComponentsResolvedFromContainer_DisposedInReverseDependencyOrder()
        {
            var target = new Container();

            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(A1)));
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(B1)));

            A1 a = target.Resolve<A1>();
            B1 b = target.Resolve<B1>();

            var disposeOrder = new Queue<object>();

            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            target.Dispose();

            // B1 depends on A1, therefore B1 should be disposed first

            Assert.AreEqual(2, disposeOrder.Count);
            Assert.AreSame(b, disposeOrder.Dequeue());
            Assert.AreSame(a, disposeOrder.Dequeue());
        }

        [Test]
        public void ComponentsResolvedFromContainerInReverseOrder_DisposedInReverseDependencyOrder()
        {
            var target = new Container();

            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(A1)));
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(B1)));

            B1 b = target.Resolve<B1>();
            A1 a = target.Resolve<A1>();

            Queue<object> disposeOrder = new Queue<object>();

            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            target.Dispose();

            // B1 depends on A1, therefore B1 should be disposed first

            Assert.AreEqual(2, disposeOrder.Count);
            Assert.AreSame(b, disposeOrder.Dequeue());
            Assert.AreSame(a, disposeOrder.Dequeue());
        }

        [Test]
        public void SingleSharedInstance_SharesOneInstanceBetweenAllLifetimeScopes()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.AreSame(ctxA, targetA);
            Assert.AreSame(ctxA2, targetA);
        }

        [Test]
        public void SingleSharedInstance_DisposedOnlyWhenContainerDisposed()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();

            Assert.IsFalse(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.IsFalse(ctxA.IsDisposed);

            container.Dispose();

            Assert.IsTrue(ctxA.IsDisposed);
        }

        [Test]
        public void NoInstanceSharing_ProvidesUniqueInstancesForAllRequests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerDependency();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.AreNotSame(ctxA, targetA);
            Assert.AreNotSame(ctxA, ctxA2);
        }

        [Test]
        public void NoInstanceSharing_DisposesInstancesWithContainingLifetime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerDependency();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsFalse(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);

            container.Dispose();

            Assert.IsTrue(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);
        }

        [Test]
        public void ShareInstanceInLifetimeScope_SharesOneInstanceInEachLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerLifetimeScope();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();

            Assert.AreSame(ctxA, ctxA2);

            var targetA = container.Resolve<A1>();
            var targetA2 = container.Resolve<A1>();

            Assert.AreSame(targetA, targetA2);
            Assert.AreNotSame(ctxA, targetA);
        }

        [Test]
        public void ShareInstanceInLifetimeScope_DisposesInstancesWithContainingScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerLifetimeScope();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsFalse(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);

            container.Dispose();

            Assert.IsTrue(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);
        }

    }
}
