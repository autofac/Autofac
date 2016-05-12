using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class MostParametersConstructorSelectorTests
    {
        [Fact]
        public void DoesNotAcceptNullBindings()
        {
            var target = new MostParametersConstructorSelector();
            Assert.Throws<ArgumentNullException>(() => target.SelectConstructorBinding(null));
        }

        [Fact]
        public void DoesNotAcceptEmptyBindings()
        {
            var target = new MostParametersConstructorSelector();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.SelectConstructorBinding(new ConstructorParameterBinding[] { }));
        }

        public class ThreeConstructors
        {
            public ThreeConstructors()
            {
            }

            public ThreeConstructors(int i, string s)
            {
            }

            public ThreeConstructors(int i)
            {
            }
        }

        [Fact]
        public void ChoosesCorrectConstructor()
        {
            var constructors = GetBindingsForAllConstructorsOf<ThreeConstructors>();
            var target = new MostParametersConstructorSelector();

            var chosen = target.SelectConstructorBinding(constructors);

            Assert.NotNull(chosen);
            Assert.Equal(2, chosen.TargetConstructor.GetParameters().Length);
        }

        private class TwoConstructors
        {
            public TwoConstructors(int i)
            {
            }

            public TwoConstructors(string s)
            {
            }
        }

        [Fact]
        public void WhenMultipleConstructorsWithTheSameLengthResolvable_ExceptionIsThrown()
        {
            var constructors = GetBindingsForAllConstructorsOf<TwoConstructors>();
            var target = new MostParametersConstructorSelector();

            Assert.Throws<DependencyResolutionException>(() => target.SelectConstructorBinding(constructors));
        }

        private static ConstructorParameterBinding[] GetBindingsForAllConstructorsOf<TTarget>()
        {
            return typeof(TTarget).GetTypeInfo().DeclaredConstructors
                .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), new ContainerBuilder().Build()))
                .ToArray();
        }
    }
}
