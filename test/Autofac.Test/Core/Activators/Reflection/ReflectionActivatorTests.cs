using System;
using System.Linq;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies;
using Autofac.Test.Scenarios.ConstructorSelection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class ReflectionActivatorTests
    {
        [Fact]
        public void Constructor_DoesNotAcceptNullType()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ReflectionActivator(null,
                    Mocks.GetConstructorFinder(),
                    Mocks.GetConstructorSelector(),
                    Factory.NoParameters,
                    Factory.NoProperties);
            });
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullParameters()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    Mocks.GetConstructorFinder(),
                    Mocks.GetConstructorSelector(),
                    null,
                    Factory.NoProperties);
            });
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullProperties()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    Mocks.GetConstructorFinder(),
                    Mocks.GetConstructorSelector(),
                    Factory.NoParameters,
                    null);
            });
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullFinder()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    null,
                    Mocks.GetConstructorSelector(),
                    Factory.NoParameters,
                    Factory.NoProperties);
            });
        }

        [Fact]
        public void Constructor_DoesNotAcceptNullSelector()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    Mocks.GetConstructorFinder(),
                    null,
                    Factory.NoParameters,
                    Factory.NoProperties);
            });
        }

        [Fact]
        public void ActivateInstance_ReturnsInstanceOfTargetType()
        {
            var target = Factory.CreateReflectionActivator(typeof(object));
            var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<object>(instance);
        }

        [Fact]
        public void ActivateInstance_ResolvesConstructorDependencies()
        {
            var o = new object();
            const string s = "s";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(o);
            builder.RegisterInstance(s);
            var container = builder.Build();

            var target = Factory.CreateReflectionActivator(typeof(Dependent));
            var instance = target.ActivateInstance(container, Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<Dependent>(instance);

            var dependent = (Dependent)instance;

            Assert.Same(o, dependent.TheObject);
            Assert.Same(s, dependent.TheString);
       }

        [Fact]
        public void ActivateInstance_DependenciesNotAvailable_ThrowsException()
        {
            var target = Factory.CreateReflectionActivator(typeof(DependsByCtor));
            var ex = Assert.Throws<DependencyResolutionException>(
                () => target.ActivateInstance(Factory.EmptyContext, Factory.NoParameters));

            // I.e. the type of the missing dependency.
            Assert.True(ex.Message.Contains("DependsByProp"));
        }

        [Fact]
        public void ByDefault_ChoosesConstructorWithMostResolvableParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(object));
            var container = builder.Build();

            var target = Factory.CreateReflectionActivator(typeof(MultipleConstructors));
            var instance = target.ActivateInstance(container, Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<MultipleConstructors>(instance);
        }

        public class AcceptsObjectParameter
        {
            public readonly object P;
            public AcceptsObjectParameter(object p) { P = p; }
        }

        [Fact]
        public void ProvidedParameters_OverrideThoseInContext()
        {
            var containedInstance = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(containedInstance);
            var container = builder.Build();

            var parameterInstance = new object();
            var parameters = new Parameter[]{ new NamedParameter("p", parameterInstance)};

            var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = (AcceptsObjectParameter)target.ActivateInstance(container, Factory.NoParameters);

            Assert.Same(parameterInstance, instance.P);
            Assert.NotSame(containedInstance, instance.P);
        }

        [Fact]
        public void WhenReferenceTypeParameterSupplied_ItIsProvidedToTheComponent()
        {
            var p = new object();
            var parameters = new Parameter[] { new NamedParameter("p", p) };

            var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = target.ActivateInstance(new ContainerBuilder().Build(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<AcceptsObjectParameter>(instance);

            var typedInstance = (AcceptsObjectParameter)instance;

            Assert.Equal(p, typedInstance.P);
        }

        [Fact]
        public void WhenNullReferenceTypeParameterSupplied_ItIsPassedToTheComponent()
        {
            var parameters = new Parameter[] { new NamedParameter("p", null) };

            var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = target.ActivateInstance(new ContainerBuilder().Build(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<AcceptsObjectParameter>(instance);

            var typedInstance = (AcceptsObjectParameter)instance;

            Assert.Null(typedInstance.P);
        }

        public class AcceptsIntParameter
        {
            public readonly int I;
            public AcceptsIntParameter(int i) { I = i; }
        }

        [Fact]
        public void WhenValueTypeParameterSupplied_ItIsPassedToTheComponent()
        {
            const int i = 42;
            var parameters = new Parameter[] { new NamedParameter("i", i) };

            var target = Factory.CreateReflectionActivator(typeof(AcceptsIntParameter), parameters);

            var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<AcceptsIntParameter>(instance);

            var typedInstance = (AcceptsIntParameter)instance;

            Assert.Equal(i, typedInstance.I);
        }

        [Fact]
        public void WhenValueTypeParameterIsSuppliedWithNull_TheDefaultForTheValueTypeIsSet()
        {
            var parameters = new Parameter[] { new NamedParameter("i", null) };

            var target = Factory.CreateReflectionActivator(typeof(AcceptsIntParameter), parameters);

            var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<AcceptsIntParameter>(instance);

            var typedInstance = (AcceptsIntParameter)instance;

            Assert.Equal(0, typedInstance.I);
        }

        public class ThreeConstructors
        {
            public readonly int CalledConstructorParameterCount;
            // ReSharper disable UnusedMember.Local, UnusedParameter.Local
            public ThreeConstructors() { CalledConstructorParameterCount = 0; }
            public ThreeConstructors(int i, string s) { CalledConstructorParameterCount = 2; }
            public ThreeConstructors(int i) { CalledConstructorParameterCount = 1; }
            // ReSharper restore UnusedMember.Local, UnusedParameter.Local
        }

        [Fact]
        public void ByDefault_ChoosesMostParameterisedConstructor()
        {
            var parameters = new Parameter[] {
                new NamedParameter("i", 1),
                new NamedParameter("s", "str")
            };

            var target = Factory.CreateReflectionActivator(typeof(ThreeConstructors), parameters);

            var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

            Assert.NotNull(instance);
            Assert.IsType<ThreeConstructors>(instance);

            var typedInstance = (ThreeConstructors)instance;

            Assert.Equal(2, typedInstance.CalledConstructorParameterCount);
        }

        public class NoPublicConstructor
        {
            // ReSharper disable EmptyConstructor
            internal NoPublicConstructor() { }
            // ReSharper restore EmptyConstructor
        }

        [Fact]
        public void NonPublicConstructorsIgnored()
        {
            var target = Factory.CreateReflectionActivator(typeof(NoPublicConstructor));
            var dx = Assert.Throws<DependencyResolutionException>(() =>
                target.ActivateInstance(new Container(), Factory.NoParameters));

            Assert.True(dx.Message.Contains(typeof(DefaultConstructorFinder).Name));
        }

        public class WithGenericCtor<T>
        {
            // ReSharper disable UnusedParameter.Local
            public WithGenericCtor(T t)
            // ReSharper restore UnusedParameter.Local
            {
            }
        }

        [Fact]
        public void CanResolveConstructorsWithGenericParameters()
        {
            var activator = Factory.CreateReflectionActivator(typeof(WithGenericCtor<string>));
            var parameters = new Parameter[] { new NamedParameter("t", "Hello") };
            var instance = activator.ActivateInstance(new Container(), parameters);
            Assert.IsType<WithGenericCtor<string>>(instance);
        }

        public class PrivateSetProperty
        {
            // ReSharper disable UnusedMember.Local
            public int GetProperty { private set; get; }
            public int P { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        [Fact]
        public void PropertiesWithPrivateSetters_AreIgnored()
        {
            var setters = new Parameter[] { new NamedPropertyParameter("P", 1) };
            var activator = Factory.CreateReflectionActivator(typeof(PrivateSetProperty), Factory.NoParameters, setters);
            var instance = activator.ActivateInstance(new Container(), Factory.NoParameters);
            Assert.IsType<PrivateSetProperty>(instance);
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public class R { public int P1 { get; set; } public int P2 { get; set; } }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        [Fact]
        public void SetsMultipleConfiguredProperties()
        {
            const int p1 = 1;
            const int p2 = 2;
            var properties = new [] {
                new NamedPropertyParameter("P1", p1),
                new NamedPropertyParameter("P2", p2)
            };
            var target = Factory.CreateReflectionActivator(typeof(R), Enumerable.Empty<Parameter>(), properties);
            var instance = (R)target.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());
            Assert.Equal(1, instance.P1);
            Assert.Equal(2, instance.P2);
        }

        public enum E { A, B }

        public class WithE
        {
            public E E { get; set; }
        }

        [Fact]
        public void EnumPropertiesCanBeAutowired()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WithE>().PropertiesAutowired();
            builder.Register(c => E.B);
            var container = builder.Build();
            var withE = container.Resolve<WithE>();
            Assert.Equal(E.B, withE.E);
        }
    }
}
