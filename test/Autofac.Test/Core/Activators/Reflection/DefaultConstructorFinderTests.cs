using System;
using System.Linq;
using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class DefaultConstructorFinderTests
    {
        [Fact]
        public void FindsPublicConstructorsOnlyByDefault()
        {
            var finder = new DefaultConstructorFinder();
            var targetType = typeof(HasConstructors);
            var publicConstructor = targetType.GetConstructor(new Type[0]);

            var constructors = finder.FindConstructors(targetType).ToList();

            Assert.Single(constructors);
            Assert.Contains(publicConstructor, constructors);
        }

        [Fact]
        public void CanFindNonPublicConstructorsUsingFinderFunction()
        {
            var finder = new DefaultConstructorFinder(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
            var targetType = typeof(HasConstructors);
            var privateConstructor = targetType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();

            var constructors = finder.FindConstructors(targetType).ToList();

            Assert.Single(constructors);
            Assert.Contains(privateConstructor, constructors);
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        internal class HasConstructors
        {
            public HasConstructors()
            {
            }

            private HasConstructors(int value)
            {
            }
        }

#pragma warning restore IDE0060

    }
}
