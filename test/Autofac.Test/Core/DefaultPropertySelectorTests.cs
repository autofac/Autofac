using System;
using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class DefaultPropertySelectorTests
    {
        [InlineData(true, "PrivatePropertyWithSet", false)]
        [InlineData(false, "PrivatePropertyWithSet", false)]
        [InlineData(true, "PublicPropertyNoDefault", true)]
        [InlineData(false, "PublicPropertyNoDefault", true)]
        [InlineData(true, "PublicPropertyWithDefault", false)]
        [InlineData(false, "PublicPropertyWithDefault", true)]
        [InlineData(true, "PublicPropertyNoGet", true)]
        [InlineData(true, "PublicPropertyNoSet", false)]
        [InlineData(true, "PublicPropertyThrowsOnGet", false)]
        [InlineData(false, "PublicPropertyThrowsOnGet", true)]
        [Theory]
        public void DefaultTests(bool preserveSetValue, string propertyName, bool expected)
        {
            var finder = new DefaultPropertySelector(preserveSetValue);

            var instance = new HasProperties();
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.Equal(expected, finder.InjectProperty(property, instance));
        }

        private class HasProperties
        {
            public Test PublicPropertyNoDefault { get; set; }

            public Test PublicPropertyNoGet
            {
                set { }
            }

            public Test PublicPropertyNoSet { get; }

            public Test PublicPropertyThrowsOnGet
            {
                get
                {
                    // Issue #799: InjectUnsetProperties blows up if the getter throws an exception.
                    throw new InvalidOperationException();
                }

                set
                {
                }
            }

            public Test PublicPropertyWithDefault { get; set; } = new Test();

            private Test PrivatePropertyWithDefault { get; set; } = new Test();

            private Test PrivatePropertyWithSet { get; set; }
        }

        private class Test
        {
        }
    }
}
