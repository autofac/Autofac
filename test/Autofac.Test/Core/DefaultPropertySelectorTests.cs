using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class DefaultPropertySelectorTests
    {
        private class Test
        {
        }

        private class HasProperties
        {
            private Test PrivatePropertyWithSet { get; set; }

            private Test PrivatePropertyWithDefault { get; set; } = new Test();

            public Test PublicPropertyNoDefault { get; set; }

            public Test PublicPropertyWithDefault { get; set; } = new Test();

            public Test PublicPropertyNoGet
            {
                set { }
            }

            public Test PublicPropertyNoSet { get; }
        }

        [InlineData(true, "PrivatePropertyWithSet", false)]
        [InlineData(false, "PrivatePropertyWithSet", false)]
        [InlineData(true, "PublicPropertyNoDefault", true)]
        [InlineData(false, "PublicPropertyNoDefault", true)]
        [InlineData(true, "PublicPropertyWithDefault", false)]
        [InlineData(false, "PublicPropertyWithDefault", true)]
        [InlineData(true, "PublicPropertyNoGet", true)]
        [InlineData(true, "PublicPropertyNoSet", false)]
        [Theory]
        public void DefaultTests(bool preserveSetValue, string propertyName, bool expected)
        {
            var finder = new DefaultPropertySelector(preserveSetValue);

            var instance = new HasProperties();
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.Equal(expected, finder.InjectProperty(property, instance));
        }
    }
}
