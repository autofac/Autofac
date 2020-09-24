// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Activators.ProvidedInstance;
using Xunit;

namespace Autofac.Test.Component.Activation
{
    public class ProvidedInstanceActivatorTests
    {
        [Fact]
        public void NullIsNotAValidInstance()
        {
            Assert.Throws<ArgumentNullException>(() => new ProvidedInstanceActivator(null));
        }

        [Fact]
        public void WhenInitializedWithInstance_ThatInstanceIsReturnedFromActivateInstance()
        {
            object instance = new object();

            ProvidedInstanceActivator target = new ProvidedInstanceActivator(instance);

            var container = Factory.CreateEmptyContainer();

            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            var actual = invoker(container, Factory.NoParameters);

            Assert.Same(instance, actual);
        }

        [Fact]
        public void ActivatingAProvidedInstanceTwice_RaisesException()
        {
            object instance = new object();

            ProvidedInstanceActivator target =
                new ProvidedInstanceActivator(instance);

            var container = Factory.CreateEmptyContainer();

            var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

            invoker(container, Factory.NoParameters);

            Assert.Throws<InvalidOperationException>(() =>
                invoker(container, Factory.NoParameters));
        }
    }
}
