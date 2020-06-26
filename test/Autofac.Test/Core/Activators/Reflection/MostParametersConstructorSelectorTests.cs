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
            Assert.Throws<ArgumentNullException>(() => target.SelectConstructorBinding(null, Enumerable.Empty<Parameter>()));
        }

        [Fact]
        public void DoesNotAcceptEmptyBindings()
        {
            var target = new MostParametersConstructorSelector();
            Assert.Throws<ArgumentOutOfRangeException>(() => target.SelectConstructorBinding(new BoundConstructor[] { }, Enumerable.Empty<Parameter>()));
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

            var chosen = target.SelectConstructorBinding(constructors, Enumerable.Empty<Parameter>());

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

            Assert.Throws<DependencyResolutionException>(() => target.SelectConstructorBinding(constructors, Enumerable.Empty<Parameter>()));
        }

        private static BoundConstructor[] GetBindingsForAllConstructorsOf<TTarget>()
        {
            return typeof(TTarget).GetTypeInfo().DeclaredConstructors
                .Select(ci => new ConstructorBinder(ci).TryBind(Enumerable.Empty<Parameter>(), new ContainerBuilder().Build()))
                .ToArray();
        }
    }
}
