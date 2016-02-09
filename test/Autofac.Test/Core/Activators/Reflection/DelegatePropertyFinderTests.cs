using Autofac.Core.Activators.Reflection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class DelegatePropertyFinderTests
    {
        private class InjectPropertyAttribute : Attribute { }

        private class HasProperties
        {
            [InjectProperty]
            public int PublicProperty { get; set; }
            public int PropNoSetter { get; }
            private int PrivateProperty { get; set; }
        }

        [Fact]
        public void ThrowsExceptionOnNull()
        {
            Assert.Throws<ArgumentNullException>("finder", () => new DelegatePropertyFinder(null));
        }

        [Fact]
        public void UsesDelegatePassedIn()
        {
            var finder = new DelegatePropertyFinder(type =>
            {
                return type.GetTypeInfo().DeclaredProperties
                    .Where(prop => prop.GetCustomAttributes<InjectPropertyAttribute>().Any())
                    .ToArray();
            });

            var targetType = typeof(HasProperties);

            var properties = finder.FindProperties(targetType);

            Assert.Equal(1, properties.Length);

            var propertyNames = properties.Select(p => p.Name);
            Assert.Equal(new[] { "PublicProperty" }, propertyNames);
        }
    }
}
