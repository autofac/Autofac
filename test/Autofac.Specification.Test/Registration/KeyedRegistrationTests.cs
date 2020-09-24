// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Features.Indexed;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class KeyedRegistrationTests
    {
        [Fact]
        public void TypeRegisteredWithKey()
        {
            var key = new object();

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Keyed<object>(key);

            var c = cb.Build();

            Assert.True(c.TryResolveKeyed(key, typeof(object), out object o1));
            Assert.NotNull(o1);
            Assert.False(c.TryResolve(typeof(object), out _));
        }

        [Fact]
        public void TypeRegisteredWithName()
        {
            var name = "object.registration";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(name);

            var c = cb.Build();

            Assert.True(c.TryResolveNamed(name, typeof(object), out object o1));
            Assert.NotNull(o1);
            Assert.False(c.TryResolve(typeof(object), out _));
        }
    }
}
