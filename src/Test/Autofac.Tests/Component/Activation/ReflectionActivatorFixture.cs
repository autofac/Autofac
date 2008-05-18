using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Component.Activation;
using NUnit.Framework;

namespace Autofac.Tests.Component.Activation
{
    [TestFixture]
    public class ReflectionActivatorFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructNull()
        {
            var target = new ReflectionActivator(null);
        }

        [Test]
        public void ActivateInstance()
        {
            var target = new ReflectionActivator(typeof(object));
            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(object), instance.GetType());
        }

        class Dependent
        {
            public object TheObject;
            public string TheString;

            public Dependent(object o, string s) {
                TheObject = o;
                TheString = s;
            }
        }

        [Test]
        public void ResolvesDependencies()
        {
            var o = new object();
            var s = "s";

			var builder = new ContainerBuilder();
			builder.Register<object>(o);
			builder.Register<string>(s);
			var container = builder.Build();

            var target = new ReflectionActivator(typeof(Dependent));
            var instance = target.ActivateInstance(container, ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(Dependent), instance);

            var dependent = (Dependent)instance;

            Assert.AreSame(o, dependent.TheObject);
            Assert.AreSame(s, dependent.TheString);
       }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void DependenciesNotAvailable()
        {
            var target = new ReflectionActivator(typeof(Dependent));
            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);
        }

        class MultipleConstructors
        {
            public MultipleConstructors(object o, string s)
            {
            }

            public MultipleConstructors(object o)
            {
            }
        }

        [Test]
        public void ChoosesAppropriateConstructor()
        {
            var o = new object();

			var builder = new ContainerBuilder();
			builder.Register<object>();
            var container = builder.Build();

            var target = new ReflectionActivator(typeof(MultipleConstructors));
            var instance = target.ActivateInstance(container, ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(MultipleConstructors), instance);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdditionalParametersNull()
        {
            var target = new ReflectionActivator(typeof(object), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AdditionalParameterEmptyName()
        {
            var args = new Dictionary<string, object>();
            args.Add("", new object());
            var target = new ReflectionActivator(typeof(object), args);
        }

        class AcceptsObjectParameter {
            public object P;
            public AcceptsObjectParameter(object p) { P = p; }
        }

        [Test]
        public void ProvidedOverridesContainer()
        {
            var containedInstance = new object();

			var builder = new ContainerBuilder();
			builder.Register(containedInstance);
			var container = builder.Build();

            var parameterInstance = new object();
            var parameters = new Dictionary<string, object>();
            parameters.Add("p", parameterInstance);

            var target = new ReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = (AcceptsObjectParameter)target.ActivateInstance(container, ActivationParameters.Empty);

            Assert.AreSame(parameterInstance, instance.P);
            Assert.AreNotSame(containedInstance, instance.P);
        }

        [Test]
        public void ExplicitReferenceTypeParameter()
        {
            var p = new object();
            var parameters = new Dictionary<string, object>();
            parameters.Add("p", p);

            var target = new ReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(AcceptsObjectParameter), instance);

            var typedInstance = (AcceptsObjectParameter)instance;

            Assert.AreEqual(p, typedInstance.P);
        }

        [Test]
        public void ExplicitReferenceTypeParameterNull()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("p", null);

            var target = new ReflectionActivator(typeof(AcceptsObjectParameter), parameters);

            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(AcceptsObjectParameter), instance);

            var typedInstance = (AcceptsObjectParameter)instance;

            Assert.IsNull(typedInstance.P);
        }

        class AcceptsIntParameter {
            public int I;
            public AcceptsIntParameter(int i) { I = i; }
        }

        [Test]
        public void ExplicitValueTypeParameter()
        {
            var i = 42;
            var parameters = new Dictionary<string, object>();
            parameters.Add("i", i);

            var target = new ReflectionActivator(typeof(AcceptsIntParameter), parameters);

            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(AcceptsIntParameter), instance);

            var typedInstance = (AcceptsIntParameter)instance;

            Assert.AreEqual(i, typedInstance.I);
        }

        [Test]
        public void ExplicitValueTypeParameterNull()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("i", null);

            var target = new ReflectionActivator(typeof(AcceptsIntParameter), parameters);

            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(AcceptsIntParameter), instance);

            var typedInstance = (AcceptsIntParameter)instance;

            Assert.AreEqual(0, typedInstance.I);
        }

        class ThreeConstructors {
            public int CalledConstructorParameterCount;
            public ThreeConstructors() { CalledConstructorParameterCount = 0; }
            public ThreeConstructors(int i, string s) { CalledConstructorParameterCount = 2; }
            public ThreeConstructors(int i) { CalledConstructorParameterCount = 1; }
        }

        [Test]
        public void DefaultChoosesMostParameterisedConstructor()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("i", 1);
            parameters.Add("s", "str");

            var target = new ReflectionActivator(typeof(ThreeConstructors), parameters);

            var instance = target.ActivateInstance(new Container(), ActivationParameters.Empty);

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(ThreeConstructors), instance);

            var typedInstance = (ThreeConstructors)instance;

            Assert.AreEqual(2, typedInstance.CalledConstructorParameterCount);
        }

        class NoPublicConstructor {
            internal NoPublicConstructor() { }
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void NonPublicConstructorsIgnored()
        {
            var target = new ReflectionActivator(typeof(NoPublicConstructor));
            target.ActivateInstance(new Container(), ActivationParameters.Empty);
        }
	}
}
