using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Test.Scenarios.Graph1;
using Xunit;

namespace Autofac.Test
{
    public class IntegrationTests
    {
        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        [Fact]
        public void ComponentsResolvedFromContainer_DisposedInReverseDependencyOrder()
        {
            var target = new Container();

            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(A1)));
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(B1)));

            var a = target.Resolve<A1>();
            var b = target.Resolve<B1>();

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

            var b = target.Resolve<B1>();
            var a = target.Resolve<A1>();

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

        public class C<T> : I1<T>, I2<T>
        {
        }
    }
}
