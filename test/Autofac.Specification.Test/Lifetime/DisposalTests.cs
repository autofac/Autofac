using System;
using System.Collections.Generic;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class DisposalTests
    {
        [Fact]
        public void ComponentsAreDisposedEvenIfCurrentScopeEndingThrowsException()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb => cb.RegisterType<DisposeTracker>().SingleInstance());

            nestedScope.CurrentScopeEnding += (sender, args) => throw new DivideByZeroException();

            var dt = nestedScope.Resolve<DisposeTracker>();

            try
            {
                nestedScope.Dispose();
            }
            catch (DivideByZeroException)
            {
            }

            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void ComponentsResolvedFromContainer_DisposedInReverseDependencyOrder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().SingleInstance();
            builder.RegisterType<B>().SingleInstance();
            var container = builder.Build();

            var a = container.Resolve<A>();
            var b = container.Resolve<B>();

            var disposeOrder = new Queue<object>();

            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            container.Dispose();

            // B1 depends on A1, therefore B1 should be disposed first
            Assert.Equal(2, disposeOrder.Count);
            Assert.Same(b, disposeOrder.Dequeue());
            Assert.Same(a, disposeOrder.Dequeue());
        }

        [Fact]
        public void ComponentsResolvedFromContainerInReverseOrder_DisposedInReverseDependencyOrder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().SingleInstance();
            builder.RegisterType<B>().SingleInstance();
            var container = builder.Build();

            var b = container.Resolve<B>();
            var a = container.Resolve<A>();

            var disposeOrder = new Queue<object>();
            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            container.Dispose();

            // B1 depends on A1, therefore B1 should be disposed first
            Assert.Equal(2, disposeOrder.Count);
            Assert.Same(b, disposeOrder.Dequeue());
            Assert.Same(a, disposeOrder.Dequeue());
        }

        [Fact]
        public void InstancesRegisteredInParentScope_ButResolvedInChild_AreDisposedWithChild()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var parent = builder.Build();
            var child = parent.BeginLifetimeScope(b => { });
            var dt = child.Resolve<DisposeTracker>();
            child.Dispose();
            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void ResolvingFromAnEndedLifetimeProducesObjectDisposedException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope();
            lifetime.Dispose();
            Assert.Throws<ObjectDisposedException>(() => lifetime.Resolve<object>());
        }

        private class A : DisposeTracker
        {
        }

        private class B : DisposeTracker
        {
            public B(A a)
            {
                this.A = a;
            }

            public A A { get; private set; }
        }
    }
}
