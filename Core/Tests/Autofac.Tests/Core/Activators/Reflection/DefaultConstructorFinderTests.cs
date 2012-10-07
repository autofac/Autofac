using System;
using System.Linq;
using System.Reflection;
using Autofac.Core.Activators.Reflection;
using NUnit.Framework;

namespace Autofac.Tests.Core.Activators.Reflection
{
    class HasConstructors
    {
        public HasConstructors() {}

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        private HasConstructors(int value) {}
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local
    }

    [TestFixture]
    public class DefaultConstructorFinderTests
    {
        [Test]
        public void FindsPublicConstructorsOnlyByDefault()
        {
            var finder = new DefaultConstructorFinder();
            var targetType = typeof(HasConstructors);
            var publicConstructor = targetType.GetConstructor(new Type[0]);

            var constructors = finder.FindConstructors(targetType).ToList();

            Assert.That(constructors, Has.Count.EqualTo(1));
            Assert.That(constructors, Has.Member(publicConstructor));
        }

        [Test]
        public void CanFindNonPublicConstructorsUsingFinderFunction()
        {
            var finder = new DefaultConstructorFinder(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
            var targetType = typeof(HasConstructors);
            var privateConstructor = targetType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(int)}, null);

            var constructors = finder.FindConstructors(targetType).ToList();

            Assert.That(constructors, Has.Count.EqualTo(1));
            Assert.That(constructors, Has.Member(privateConstructor));
        }
    }
}
