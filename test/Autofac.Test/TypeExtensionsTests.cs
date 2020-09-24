// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Test.Scenarios.ScannedAssembly;
using Xunit;

namespace Autofac.Test
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void IsClosedTypeOfNonGenericTypeProvidedThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                typeof(object).IsClosedTypeOf(typeof(SaveCommandData)));
        }

        [Fact]
        public void IsClosedTypeOfClosedGenericTypeProvidedThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() =>
                typeof(object).IsClosedTypeOf(typeof(ICommand<SaveCommandData>)));
        }

        [Fact]
        public void IsClosedTypeOfReturnsTrueForOpenGenericInterfaces()
        {
            Assert.True(typeof(ICommand<SaveCommandData>).IsClosedTypeOf(typeof(ICommand<>)));
        }

        [Fact]
        public void IsClosedTypeOfReturnsTrueForClosedClasses()
        {
            Assert.True(typeof(SaveCommand).IsClosedTypeOf(typeof(ICommand<>)));
        }

        [Fact]
        public void AnOpenGenericTypeIsNotAClosedTypeOfAnything()
        {
            Assert.False(typeof(CommandBase<>).IsClosedTypeOf(typeof(CommandBase<>)));
        }
    }
}
