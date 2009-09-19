using System;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class MostParametersConstructorSelectorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoesNotAcceptNullBindings()
        {
            var target = new MostParametersConstructorSelector();
            target.SelectConstructorBinding(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DoesNotAcceptEmptyBindings()
        {
            var target = new MostParametersConstructorSelector();
            target.SelectConstructorBinding(new ConstructorParameterBinding[] { });
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
            var constructors = typeof(ThreeConstructors).GetConstructors()
                .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), Container.Empty));

            Assert.AreEqual(3, constructors.Count());

            var target = new MostParametersConstructorSelector();

            var chosen = target.SelectConstructorBinding(constructors);

            Assert.IsNotNull(chosen);
            Assert.AreEqual(2, chosen.TargetConstructor.GetParameters().Length);
        }
    }
}
