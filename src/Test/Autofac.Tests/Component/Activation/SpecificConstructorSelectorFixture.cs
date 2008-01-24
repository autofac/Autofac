using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Activation;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class SpecificConstructorSelectorFixture
    {
        class ThreeConstructors
        {
            public ThreeConstructors() { }
            public ThreeConstructors(int i) { }
            public ThreeConstructors(int i, string s) { }
        }

        [Test]
        public void SelectsConstructor()
        {
            var ctors = typeof(ThreeConstructors).GetConstructors();

            var target0 = new SpecificConstructorSelector();
            var c0 = target0.SelectConstructor(ctors);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.GetParameters().Length);

            var target2 = new SpecificConstructorSelector(typeof(int), typeof(string));
            var c2 = target2.SelectConstructor(ctors);
            Assert.IsNotNull(c2);
            Assert.AreEqual(2, c2.GetParameters().Length);
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void ThrowsWhenNotAvailable()
        {
            var ctors = typeof(ThreeConstructors).GetConstructors();
            var target = new SpecificConstructorSelector(typeof(string));
            target.SelectConstructor(ctors);
        }
    }
}
