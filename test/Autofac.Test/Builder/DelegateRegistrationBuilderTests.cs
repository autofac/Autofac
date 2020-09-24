// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Builder
{
    public class DelegateRegistrationBuilderTests
    {
        [Fact]
        public void RegisterNull()
        {
            var target = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => target.Register((Func<IComponentContext, object>)null));
        }

        [Fact]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.Register(c => "Hello").As<object>();
            var container = cb.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out IComponentRegistration cr));
            Assert.Equal(typeof(string), cr.Activator.LimitType);
        }
    }
}
