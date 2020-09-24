// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Features.OwnedInstances
{
    public class InstancePerOwnedKeyTests
    {
        [Theory]
        [InlineData(typeof(string), "Foo", typeof(string), true)]
        [InlineData(typeof(string), null, typeof(string), true)]
        [InlineData(typeof(string), "Foo", typeof(int), false)]
        [InlineData(typeof(string), null, typeof(int), false)]
        [InlineData(typeof(int), "Foo", typeof(string), false)]
        [InlineData(typeof(int), null, typeof(string), false)]
        public void ServiceEquality(Type dependencyType, object ownedKey, Type ownedType, bool expected)
        {
            var dependencyService = new TypedService(dependencyType);
            var instancePerOwnedKey = new InstancePerOwnedKey(dependencyService);

            var ownedService = ownedKey != null
                ? (IServiceWithType)new KeyedService(ownedKey, ownedType)
                : new TypedService(ownedType);

            Assert.Equal(expected, instancePerOwnedKey.Equals(ownedService));
        }
    }
}
