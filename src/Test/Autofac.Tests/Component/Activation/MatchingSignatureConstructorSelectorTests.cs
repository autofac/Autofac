using NUnit.Framework;
using Autofac.Activators;
using Autofac.Injection;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class MatchingSignatureConstructorSelectorTests
    {
        class ThreeConstructors
        {
            public ThreeConstructors() { }
            public ThreeConstructors(int i) { }
            public ThreeConstructors(int i, string s) { }
        }

        IEnumerable<ConstructorParameterBinding> _ctors = typeof(ThreeConstructors)
            .GetConstructors()
            .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), Container.Empty));


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
        [ExpectedException(typeof(DependencyResolutionException))]
        public void ThrowsWhenNotAvailable()
        {
            var target = new MatchingSignatureConstructorSelector(typeof(string));
            target.SelectConstructorBinding(_ctors);
        }
    }
}
