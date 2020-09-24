// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class DelegatePropertySelectorTests
    {
        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0051

        private class InjectPropertyAttribute : Attribute
        {
        }

        private class HasProperties
        {
            [InjectProperty]
            public int PublicProperty { get; set; }

            public int PropNoSetter { get; }

            private int PrivateProperty { get; set; }
        }

#pragma warning restore IDE0051

        [Fact]
        public void ThrowsExceptionOnNull()
        {
            Assert.Throws<ArgumentNullException>("finder", () => new DelegatePropertySelector(null));
        }

        [Fact]
        public void UsesDelegatePassedIn()
        {
            var finder = new DelegatePropertySelector((propInfo, instance) =>
            {
                return propInfo.GetCustomAttributes<InjectPropertyAttribute>().Any();
            });

            foreach (var propInfo in typeof(HasProperties).GetProperties())
            {
                var expected = propInfo.GetCustomAttributes<InjectPropertyAttribute>().Any();
                Assert.Equal(expected, finder.InjectProperty(propInfo, null));
            }
        }
    }
}
