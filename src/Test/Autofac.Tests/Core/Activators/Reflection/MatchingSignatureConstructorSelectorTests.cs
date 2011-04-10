using NUnit.Framework;
using System.Linq;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;

namespace Autofac.Tests.Core.Activators.Reflection
{
    [TestFixture]
    public class MatchingSignatureConstructorSelectorTests
    {
        class ThreeConstructors
        {
            // ReSharper disable UnusedMember.Local, UnusedParameter.Local
            public ThreeConstructors() { }
            public ThreeConstructors(int i) { }
            public ThreeConstructors(int i, string s) { }
            // ReSharper restore UnusedMember.Local, UnusedParameter.Local
        }

        readonly ConstructorParameterBinding[] _ctors = typeof(ThreeConstructors)
            .GetConstructors()
            .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), Container.Empty))
            .ToArray();

        [Test]
        public void SelectsEmptyConstructor()
        {
            var target0 = new MatchingSignatureConstructorSelector();
            var c0 = target0.SelectConstructorBinding(_ctors);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.TargetConstructor.GetParameters().Length);
        }

        [Test]
        public void SelectsConstructorWithParameters()
        {
            var target2 = new MatchingSignatureConstructorSelector(typeof(int), typeof(string));
            var c2 = target2.SelectConstructorBinding(_ctors);
            Assert.IsNotNull(c2);
            Assert.AreEqual(2, c2.TargetConstructor.GetParameters().Length);
        }

        [Test]
        public void WhenNoMatchingConstructorsAvailable_ExceptionDescribesTargetTypeAndSignature()
        {
            var target = new MatchingSignatureConstructorSelector(typeof(string));

            var dx = Assert.Throws<DependencyResolutionException>(() =>
                target.SelectConstructorBinding(_ctors));

            Assert.That(dx.Message.Contains(typeof(ThreeConstructors).Name));
            Assert.That(dx.Message.Contains(typeof(string).Name));
        }
    }
}
