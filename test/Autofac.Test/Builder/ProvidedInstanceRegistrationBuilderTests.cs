// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Builder
{
    public class ProvidedInstanceRegistrationBuilderTests
    {
        [Fact]
        public void LimitType_ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello").As<object>();
            var container = cb.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out IComponentRegistration cr));
            Assert.Equal(typeof(string), cr.Activator.LimitType);
        }

        [Fact]
        public void DefaultServiceType_IsStaticTypeOfRegisteredInstance()
        {
            object instance = "Hello";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance);
            var container = builder.Build();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<string>();
        }
    }
}
