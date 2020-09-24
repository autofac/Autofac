// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        // Issue #960
        [Fact]
        public void TwoLayersOfExternalRegistration_OnDisposingInnerLayer_OuterLayerRemains()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>().InstancePerLifetimeScope();

            // Root has the main registration.
            var root = builder.Build();
            var rootInstance = root.Resolve<DisposeTracker>();

            // Middle has ExternalRegistration pointing upwards.
            var middle = root.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            var middleInstance = middle.Resolve<DisposeTracker>();

            // Child has ExternalRegistration pointing upwards.
            var child = middle.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            var childInstance = child.Resolve<DisposeTracker>();

            Assert.NotSame(rootInstance, middleInstance);
            Assert.NotSame(middleInstance, childInstance);
            Assert.NotSame(rootInstance, childInstance);

            // This should only dispose the registration in child, not the one in middle or root.
            child.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.False(middleInstance.IsDisposed);
            Assert.False(rootInstance.IsDisposed);

            // We check by trying to use the registration in middle.
            // If too much got disposed, this will throw ObjectDisposedException.
            Assert.Same(middleInstance, middle.Resolve<DisposeTracker>());
            Assert.Same(rootInstance, root.Resolve<DisposeTracker>());

            // Middle and child should now be disposed.
            middle.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.True(middleInstance.IsDisposed);
            Assert.False(rootInstance.IsDisposed);

            // Now all should be disposed.
            root.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.True(middleInstance.IsDisposed);
            Assert.True(rootInstance.IsDisposed);
        }

        [Fact]
        public void DisposalOfContainerPreventsResolveInLifetimeScope()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(c => 1);

            var container = containerBuilder.Build();

            var scope = container.BeginLifetimeScope();

            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                scope.Resolve<int>();
            });
        }

        [Fact]
        public void DisposalOfParentScopePreventsResolveInNestedScope()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(c => 1);

            var container = containerBuilder.Build();

            var outerScope = container.BeginLifetimeScope();
            var innerScope = outerScope.BeginLifetimeScope();

            outerScope.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                innerScope.Resolve<int>();
            });
        }

        [Fact]
        public void DisposalOfContainerPreventsResolveInNestedScope()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(c => 1);

            var container = containerBuilder.Build();

            var outerScope = container.BeginLifetimeScope();
            var innerScope = outerScope.BeginLifetimeScope();

            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                innerScope.Resolve<int>();
            });
        }

        private class A : DisposeTracker
        {
        }

        private class B : DisposeTracker
        {
            public B(A a)
            {
                A = a;
            }

            public A A { get; private set; }
        }
    }
}
