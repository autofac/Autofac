// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        [Fact]
        public void ChoosesCorrectConstructor()
        {
            var constructors = GetBindingsForAllConstructorsOf<ThreeConstructors>();
            var target = new MostParametersConstructorSelector();

            var chosen = target.SelectConstructorBinding(constructors, Enumerable.Empty<Parameter>());

            Assert.Equal(2, chosen.TargetConstructor.GetParameters().Length);
        }

        [Fact]
        public void IgnoresInvalidConstructor()
        {
            var constructors = GetBindingsForAllConstructorsOf<OneValidConstructorOneInvalid>();
            var target = new MostParametersConstructorSelector();

            var chosen = target.SelectConstructorBinding(constructors, Enumerable.Empty<Parameter>());

            Assert.Single(chosen.TargetConstructor.GetParameters());
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
            var builder = new ContainerBuilder();
            builder.RegisterInstance("test");
            builder.Register(ctx => 1);
            var container = builder.Build();

            return typeof(TTarget).GetTypeInfo().DeclaredConstructors
                .Select(ci => new ConstructorBinder(ci).Bind(new[] { new AutowiringParameter() }, container))
                .ToArray();
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

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

        public class OneValidConstructorOneInvalid
        {
            public OneValidConstructorOneInvalid(int i)
            {
            }

            public OneValidConstructorOneInvalid(int i2, object bad)
            {
            }
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

#pragma warning restore IDE0060

    }
}
