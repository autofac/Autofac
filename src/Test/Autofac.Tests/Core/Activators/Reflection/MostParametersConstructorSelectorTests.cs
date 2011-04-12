using System;
using NUnit.Framework;
using System.Linq;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;

namespace Autofac.Tests.Core.Activators.Reflection
{
    [TestFixture]
    public class MostParametersConstructorSelectorFixture
    {
        // ReSharper disable ClassNeverInstantiated.Local

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
            var constructors = GetBindingsForAllConstructorsOf<ThreeConstructors>();
            var target = new MostParametersConstructorSelector();
            
            var chosen = target.SelectConstructorBinding(constructors);

            Assert.IsNotNull(chosen);
            Assert.AreEqual(2, chosen.TargetConstructor.GetParameters().Length);
        }

        class TwoConstructors
        {
            public TwoConstructors(int i) { }
            public TwoConstructors(string s) { }
        }

        [Test]
        public void WhenMultipleConstructorsWithTheSameLengthResolvable_ExceptionIsThrown()
        {
            var constructors = GetBindingsForAllConstructorsOf<TwoConstructors>();
            var target = new MostParametersConstructorSelector();

            Assert.Throws<DependencyResolutionException>(() => target.SelectConstructorBinding(constructors));
        }

        static ConstructorParameterBinding[] GetBindingsForAllConstructorsOf<TTarget>()
        {
            return typeof(TTarget).GetConstructors()
                .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), Container.Empty))
                .ToArray();
        }

        // ReSharper restore ClassNeverInstantiated.Local
    }
}
