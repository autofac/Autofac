// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Xunit;

namespace Autofac.Test.Component.Activation
{
    public class DelegateActivatorTests
    {
        [Fact]
        public void Constructor_DoesNotAcceptNullDelegate()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateActivator(typeof(object), null));
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullType()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateActivator(null, (c, p) => new object()));
        }

        [Fact]
        public void Pipeline_ReturnsResultOfInvokingSuppliedDelegate()
        {
            var instance = new object();

            var target =
                new DelegateActivator(typeof(object), (c, p) => instance);

            var container = Factory.CreateEmptyContainer();
            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            Assert.Same(instance, invoker(container, Factory.NoParameters));
        }

        [Fact]
        public void WhenActivationDelegateReturnsNull_ExceptionDescribesLimitType()
        {
            var target = new DelegateActivator(typeof(string), (c, p) => null);

            var container = Factory.CreateEmptyContainer();
            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            var ex = Assert.Throws<DependencyResolutionException>(
                () => invoker(container, Factory.NoParameters));

            Assert.Contains(typeof(string).ToString(), ex.Message);
        }
    }
}
