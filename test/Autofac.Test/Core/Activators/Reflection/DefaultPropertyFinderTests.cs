using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class DefaultPropertyFinderTests
    {
        private class HasProperties
        {
            public int PublicProperty { get; set; }
            public int PropNoSetter { get; }
            private int PrivateProperty { get; set; }
        }

        [Fact]
        public void DoesNotFindPropertiesByDefault()
        {
            var finder = new DefaultPropertyFinder();
            var targetType = typeof(HasProperties);

            var properties = finder.FindProperties(targetType);

            Assert.Empty(properties);
        }
    }
}
