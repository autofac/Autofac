using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies.Circularity;
using Autofac.Test.Scenarios.Graph1;
using Xunit;

namespace Autofac.Test
{
    public class IntegrationTests
    {
        [Fact]
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

            Assert.IsType<CD1>(c);
            CD1 cd = (CD1)c;

            Assert.Same(a, b.A);
            Assert.Same(a, cd.A);
            Assert.NotSame(b, cd.B);
            Assert.Same(c, e.C);
            Assert.NotSame(b, e.B);
            Assert.NotSame(e.B, cd.B);
        }

        [Fact]
        public void DetectsAndIdentifiesCircularDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<D>().As<ID>();
            builder.RegisterType<A>().As<IA>();
            builder.RegisterType<BC>().As<IB, IC>();

            var container = builder.Build();

            var de = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ID>());
            Assert.True(de.Message.Contains(
                "Autofac.Test.Scenarios.Dependencies.Circularity.D -> Autofac.Test.Scenarios.Dependencies.Circularity.A -> Autofac.Test.Scenarios.Dependencies.Circularity.BC -> Autofac.Test.Scenarios.Dependencies.Circularity.A"));
        }

        [Fact]
        public void UnresolvedProvidedInstances_DisposedWithLifetimeScope()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Dispose();
            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void ResolvedProvidedInstances_OnlyDisposedOnce()
        {
            // Issue 383: Disposing a container should only dispose a provided instance one time.
            var builder = new ContainerBuilder();
            var count = 0;
            var disposable = new DisposeTracker();
            disposable.Disposing += (sender, e) => count++;
            builder.RegisterInstance(disposable);
            var container = builder.Build();
            container.Resolve<DisposeTracker>();
            container.Dispose();
            Assert.Equal(1, count);
        }

        [Fact]
        public void UnresolvedProvidedInstances_NotOwnedByLifetimeScope_NeverDisposed()
        {
            var builder = new ContainerBuilder();
            var disposable = new DisposeTracker();
            builder.RegisterInstance(disposable).ExternallyOwned();
            var container = builder.Build();
            container.Dispose();
            Assert.False(disposable.IsDisposed);
        }

        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        public class C<T> : I1<T>, I2<T>
        {
        }

        [Fact]
        public void MultipleServicesOnAnOpenGenericType_ShareTheSameRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(C<>)).As(typeof(I1<>), typeof(I2<>));
            var container = builder.Build();
            container.Resolve<I1<int>>();
            var count = container.ComponentRegistry.Registrations.Count();
            container.Resolve<I2<int>>();
            Assert.Equal(count, container.ComponentRegistry.Registrations.Count());
        }

        [Fact]
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
            Assert.Equal(2, disposeOrder.Count);
            Assert.Same(b, disposeOrder.Dequeue());
            Assert.Same(a, disposeOrder.Dequeue());
        }

        [Fact]
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
            Assert.Equal(2, disposeOrder.Count);
            Assert.Same(b, disposeOrder.Dequeue());
            Assert.Same(a, disposeOrder.Dequeue());
        }

        [Fact]
        public void SingleSharedInstance_SharesOneInstanceBetweenAllLifetimeScopes()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.Same(ctxA, targetA);
            Assert.Same(ctxA2, targetA);
        }

        [Fact]
        public void SingleSharedInstance_DisposedOnlyWhenContainerDisposed()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();

            Assert.False(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.False(ctxA.IsDisposed);

            container.Dispose();

            Assert.True(ctxA.IsDisposed);
        }

        [Fact]
        public void NoInstanceSharing_ProvidesUniqueInstancesForAllRequests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerDependency();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.NotSame(ctxA, targetA);
            Assert.NotSame(ctxA, ctxA2);
        }

        [Fact]
        public void NoInstanceSharing_DisposesInstancesWithContainingLifetime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerDependency();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.False(targetA.IsDisposed);
            Assert.False(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.False(targetA.IsDisposed);
            Assert.True(ctxA.IsDisposed);

            container.Dispose();

            Assert.True(targetA.IsDisposed);
            Assert.True(ctxA.IsDisposed);
        }

        [Fact]
        public void ShareInstanceInLifetimeScope_SharesOneInstanceInEachLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerLifetimeScope();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var ctxA2 = lifetime.Resolve<A1>();

            Assert.Same(ctxA, ctxA2);

            var targetA = container.Resolve<A1>();
            var targetA2 = container.Resolve<A1>();

            Assert.Same(targetA, targetA2);
            Assert.NotSame(ctxA, targetA);
        }

        [Fact]
        public void ShareInstanceInLifetimeScope_DisposesInstancesWithContainingScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().InstancePerLifetimeScope();

            var container = builder.Build();

            var lifetime = container.BeginLifetimeScope();

            var ctxA = lifetime.Resolve<A1>();
            var targetA = container.Resolve<A1>();

            Assert.False(targetA.IsDisposed);
            Assert.False(ctxA.IsDisposed);

            lifetime.Dispose();

            Assert.False(targetA.IsDisposed);
            Assert.True(ctxA.IsDisposed);

            container.Dispose();

            Assert.True(targetA.IsDisposed);
            Assert.True(ctxA.IsDisposed);
        }
    }
}
