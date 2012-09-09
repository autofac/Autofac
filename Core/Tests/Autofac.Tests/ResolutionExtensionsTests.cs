using System;
using NUnit.Framework;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Tests.Scenarios.Parameterisation;
using Autofac.Core.Registration;

namespace Autofac.Tests
{
    [TestFixture]
    public class ResolutionExtensionsTests
    {
        [Test]
        public void ResolvingUnregisteredService_ProvidesDescriptionInException()
        {
            var target = new Container();
            var ex = Assert.Throws<ComponentNotRegisteredException>(() => target.Resolve<object>());
            Assert.IsTrue(ex.Message.Contains("System.Object"));
        }

        [Test]
        public void WhenComponentIsRegistered_IsRegisteredReturnsTrueForAllServices()
        {
            var registration = Factory.CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)), new TypedService(typeof(string)) },
                Factory.CreateProvidedInstanceActivator("Hello"));

            var target = new Container();

            target.ComponentRegistry.Register(registration);

            Assert.IsTrue(target.IsRegistered<object>());
            Assert.IsTrue(target.IsRegistered<string>());
        }

        [Test]
        public void WhenServiceIsRegistered_ResolveOptionalReturnsAnInstance()
        {
            var target = new Container();
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(
                new[] { new TypedService(typeof(string)) },
                new ProvidedInstanceActivator("Hello")));

            var inst = target.ResolveOptional<string>();

            Assert.AreEqual("Hello", inst);
        }

        [Test]
        public void WhenServiceNotRegistered_ResolveOptionalReturnsNull()
        {
            var target = new Container();
            var inst = target.ResolveOptional<string>();
            Assert.IsNull(inst);
        }

        [Test]
        public void WhenParametersProvided_ResolveOptionalSuppliesThemToComponent()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Parameterised>();
            var container = cb.Build();
            const string param1 = "Hello";
            const int param2 = 42;
            var result = container.ResolveOptional<Parameterised>(
                new NamedParameter("a", param1),
                new NamedParameter("b", param2));
            Assert.IsNotNull(result);
            Assert.AreEqual(param1, result.A);
            Assert.AreEqual(param2, result.B);
        }

        [Test]
        public void WhenPredicateAndValueParameterSupplied_PassedToComponent()
        {
            const string a = "Hello";
            const int b = 42;
            var builder = new ContainerBuilder();
            
            builder.RegisterType<Parameterised>()
                .WithParameter(
                    (pi, c) => pi.Name == "a",
                    (pi, c) => a)
                .WithParameter(
                    (pi, c) => pi.Name == "b",
                    (pi, c) => b);

            var container = builder.Build();
            var result = container.Resolve<Parameterised>();

            Assert.AreEqual(a, result.A);
            Assert.AreEqual(b, result.B);            
        }
    }
}
