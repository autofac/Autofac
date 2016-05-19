using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class MatchingSignatureConstructorSelectorTests
    {
        public class ThreeConstructors
        {
            public ThreeConstructors()
            {
            }

            public ThreeConstructors(int i)
            {
            }

            public ThreeConstructors(int i, string s)
            {
            }
        }

        private readonly ConstructorParameterBinding[] _ctors = typeof(ThreeConstructors)
            .GetTypeInfo().DeclaredConstructors
            .Select(ci => new ConstructorParameterBinding(ci, Enumerable.Empty<Parameter>(), new ContainerBuilder().Build()))
            .ToArray();

        [Fact]
        public void SelectsEmptyConstructor()
        {
            var target0 = new MatchingSignatureConstructorSelector();
            var c0 = target0.SelectConstructorBinding(_ctors);
            Assert.NotNull(c0);
            Assert.Equal(0, c0.TargetConstructor.GetParameters().Length);
        }

        [Fact]
        public void SelectsConstructorWithParameters()
        {
            var target2 = new MatchingSignatureConstructorSelector(typeof(int), typeof(string));
            var c2 = target2.SelectConstructorBinding(_ctors);
            Assert.NotNull(c2);
            Assert.Equal(2, c2.TargetConstructor.GetParameters().Length);
        }

        [Fact]
        public void WhenNoMatchingConstructorsAvailable_ExceptionDescribesTargetTypeAndSignature()
        {
            var target = new MatchingSignatureConstructorSelector(typeof(string));

            var dx = Assert.Throws<DependencyResolutionException>(() =>
                target.SelectConstructorBinding(_ctors));

            Assert.True(dx.Message.Contains(typeof(ThreeConstructors).Name));
            Assert.True(dx.Message.Contains(typeof(string).Name));
        }
    }
}
