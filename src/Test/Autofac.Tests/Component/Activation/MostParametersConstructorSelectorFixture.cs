using System;
using System.Reflection;
using Autofac.Component.Activation;
using NUnit.Framework;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class MostParametersConstructorSelectorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PossibleConstructorsNull()
        {
            var target = new MostParametersConstructorSelector();
            target.SelectConstructor(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PossibleConstructorsEmpty()
        {
            var target = new MostParametersConstructorSelector();
            target.SelectConstructor(new ConstructorInfo[] { });
        }

        class ThreeConstructors
        {
            public ThreeConstructors() { }
            public ThreeConstructors(int i, string s) { }
            public ThreeConstructors(int i) { }
        }

        [Test]
        public void ChoosesCorrectConstructor()
        {
            var constructors = typeof(ThreeConstructors).GetConstructors();

            Assert.AreEqual(3, constructors.Length);

            var target = new MostParametersConstructorSelector();

            var chosen = target.SelectConstructor(constructors);

            Assert.IsNotNull(chosen);
            Assert.AreEqual(2, chosen.GetParameters().Length);
        }
    }
}
