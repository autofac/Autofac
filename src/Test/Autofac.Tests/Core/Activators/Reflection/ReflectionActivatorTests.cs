using System;
using Autofac.Builder;
using NUnit.Framework;
using System.Linq;
using Moq;
using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Autofac.Core;
using Autofac.Tests.Scenarios.Dependencies;
using Autofac.Tests.Scenarios.ConstructorSelection;

namespace Autofac.Tests.Core.Activators.Reflection
{
    [TestFixture]
    public class ReflectionActivatorFixture
    {
        [Test]
        public void Constructor_DoesNotAcceptNullType()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new ReflectionActivator(null,
                    new Mock<IConstructorFinder>().Object,
                    new Mock<IConstructorSelector>().Object,
                    Factory.NoParameters);
            });
        }

        [Test]
        public void Constructor_DoesNotAcceptNullParameters()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    new Mock<IConstructorFinder>().Object,
                    new Mock<IConstructorSelector>().Object,
                    null);
            });
        }

        [Test]
        public void Constructor_DoesNotAcceptNullFinder()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    null,
                    new Mock<IConstructorSelector>().Object,
                    Factory.NoParameters);
            });
        }

        [Test]
        public void Constructor_DoesNotAcceptNullSelector()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new ReflectionActivator(typeof(object),
                    new Mock<IConstructorFinder>().Object,
                    null,
                    Factory.NoParameters);
            });
        }

        [Test]
        public void ActivateInstance_ReturnsInstanceOfTargetType()
        {
            var target = Factory.CreateReflectionActivator(typeof(object));
            var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(object), instance.GetType());
        }

        [Test]
        public void ActivateInstance_ResolvesConstructorDependencies()
        {
            var o = new object();
            var s = "s";

			var builder = new ContainerBuilder();
			builder.RegisterInstance<object>(o);
			builder.RegisterInstance<string>(s);
			var container = builder.Build();

            var target = Factory.CreateReflectionActivator(typeof(Dependent));
            var instance = target.ActivateInstance(container, Factory.NoParameters);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(Dependent), instance);

            var dependent = (Dependent)instance;

            Assert.AreSame(o, dependent.TheObject);
            Assert.AreSame(s, dependent.TheString);
       }

        [Test]
        public void ActivateInstance_DependenciesNotAvailable_ThrowsException()
        {
            var target = Factory.CreateReflectionActivator(typeof(Dependent));
            Assertions.AssertThrows<DependencyResolutionException>(delegate
            {
                target.ActivateInstance(Factory.EmptyContext, Factory.NoParameters);
            });
        }

        [Test]
        public void ChoosesAppropriateConstructor()
        {
            var o = new object();

			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(object));
            var container = builder.Build();

            var target = Factory.CreateReflectionActivator(typeof(MultipleConstructors));
            var instance = target.ActivateInstance(container, Factory.NoParameters);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(MultipleConstructors), instance);
        }

        //class AcceptsObjectParameter {
        //    public object P;
        //    public AcceptsObjectParameter(object p) { P = p; }
        //}

        //[Test]
        //public void ProvidedOverridesContainer()
        //{
        //    var containedInstance = new object();

        //    var builder = new ContainerBuilder();
        //    builder.RegisterInstance(containedInstance);
        //    var container = builder.Build();

        //    var parameterInstance = new object();
        //    var parameters = new Parameter[]{ new NamedParameter("p", parameterInstance)};

        //    var target = CreateActivator(typeof(AcceptsObjectParameter));
        //    target., parameters);

        //    var instance = (AcceptsObjectParameter)target.ActivateInstance(container, Factory.NoParameters);

        //    Assert.AreSame(parameterInstance, instance.P);
        //    Assert.AreNotSame(containedInstance, instance.P);
        //}

        //[Test]
        //public void ExplicitReferenceTypeParameter()
        //{
        //    var p = new object();
        //    var parameters = new Parameter[] { new NamedParameter("p", p) };

        //    var target = CreateActivator(typeof(AcceptsObjectParameter), parameters);

        //    var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

        //    Assert.IsNotNull(instance);
        //    Assert.IsInstanceOfType(typeof(AcceptsObjectParameter), instance);

        //    var typedInstance = (AcceptsObjectParameter)instance;

        //    Assert.AreEqual(p, typedInstance.P);
        //}

        //[Test]
        //public void ExplicitReferenceTypeParameterNull()
        //{
        //    var parameters = new Parameter[] { new NamedParameter("p", null) };

        //    var target = CreateActivator(typeof(AcceptsObjectParameter), parameters);

        //    var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

        //    Assert.IsNotNull(instance);
        //    Assert.IsInstanceOfType(typeof(AcceptsObjectParameter), instance);

        //    var typedInstance = (AcceptsObjectParameter)instance;

        //    Assert.IsNull(typedInstance.P);
        //}

        //class AcceptsIntParameter {
        //    public int I;
        //    public AcceptsIntParameter(int i) { I = i; }
        //}

        //[Test]
        //public void ExplicitValueTypeParameter()
        //{
        //    var i = 42;
        //    var parameters = new Parameter[] { new NamedParameter("i", i) };

        //    var target = CreateActivator(typeof(AcceptsIntParameter), parameters);

        //    var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

        //    Assert.IsNotNull(instance);
        //    Assert.IsInstanceOfType(typeof(AcceptsIntParameter), instance);

        //    var typedInstance = (AcceptsIntParameter)instance;

        //    Assert.AreEqual(i, typedInstance.I);
        //}

        //[Test]
        //public void ExplicitValueTypeParameterNull()
        //{
        //    var parameters = new Parameter[] { new NamedParameter("i", null) };

        //    var target = CreateActivator(typeof(AcceptsIntParameter), parameters);

        //    var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

        //    Assert.IsNotNull(instance);
        //    Assert.IsInstanceOfType(typeof(AcceptsIntParameter), instance);

        //    var typedInstance = (AcceptsIntParameter)instance;

        //    Assert.AreEqual(0, typedInstance.I);
        //}

        //class ThreeConstructors {
        //    public int CalledConstructorParameterCount;
        //    public ThreeConstructors() { CalledConstructorParameterCount = 0; }
        //    public ThreeConstructors(int i, string s) { CalledConstructorParameterCount = 2; }
        //    public ThreeConstructors(int i) { CalledConstructorParameterCount = 1; }
        //}

        //[Test]
        //public void DefaultChoosesMostParameterisedConstructor()
        //{
        //    var parameters = new Parameter[] {
        //        new NamedParameter("i", 1),
        //        new NamedParameter("s", "str")
        //    };

        //    var target = CreateActivator(typeof(ThreeConstructors), parameters);

        //    var instance = target.ActivateInstance(new Container(), Factory.NoParameters);

        //    Assert.IsNotNull(instance);
        //    Assert.IsInstanceOfType(typeof(ThreeConstructors), instance);

        //    var typedInstance = (ThreeConstructors)instance;

        //    Assert.AreEqual(2, typedInstance.CalledConstructorParameterCount);
        //}

        //class NoPublicConstructor {
        //    internal NoPublicConstructor() { }
        //}

        //[Test]
        //[ExpectedException(typeof(DependencyResolutionException))]
        //public void NonPublicConstructorsIgnored()
        //{
        //    var target = CreateActivator(typeof(NoPublicConstructor));
        //    target.ActivateInstance(new Container(), Factory.NoParameters);
        //}

        //public class WithGenericCtor<T>
        //{
        //    public WithGenericCtor(T t)
        //    {
        //    }
        //}

        //[Test]
        //public void CanResolveConstructorsWithGenericParameters()
        //{
        //    var activator = CreateActivator(typeof(WithGenericCtor<string>));
        //    var parameters = new Parameter[] { new NamedParameter("t", "Hello") };
        //    var instance = activator.ActivateInstance(new Container(), parameters);
        //    Assert.IsInstanceOfType(typeof(WithGenericCtor<string>), instance);
        //}

        //class PrivateSetProperty {
        //    public int GetProperty { private set; get; }
        //    public int P { get; set; }
        //}

        //[Test]
        //public void CanDealWithPrivateSetProperties()
        //{
        //    var setters = new[]{new NamedPropertyParameter("P", 1)};
        //    var activator = CreateActivator(typeof(PrivateSetProperty), Factory.NoParameters, setters);
        //    var instance = activator.ActivateInstance(new Container(), Factory.NoParameters);
        //    Assert.IsInstanceOfType(typeof(PrivateSetProperty), instance);
        //}

        //class ThrowsExceptionInCtor
        //{
        //    public ThrowsExceptionInCtor()
        //    {
        //        WeThrowThis();
        //    }

        //    static void WeThrowThis()
        //    {
        //        throw new InvalidOperationException();
        //    }
        //}

        //[Test]
        //public void StackTraceIsPreserved()
        //{
        //    try
        //    {
        //        var target = CreateActivator(typeof (ThrowsExceptionInCtor));
        //        target.ActivateInstance(new Container(), Factory.NoParameters);
        //        Assert.Fail();
        //    }
        //    catch(InvalidOperationException ex)
        //    {
        //        StringAssert.Contains("WeThrowThis", ex.StackTrace);
        //    }
        //}
	}
}
