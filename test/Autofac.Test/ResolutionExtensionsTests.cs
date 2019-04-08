using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.Parameterisation;
using Xunit;

namespace Autofac.Test
{
    public class ResolutionExtensionsTests
    {
        [Fact]
        public void ResolvingUnregisteredService_ProvidesDescriptionInException()
        {
            var target = Factory.EmptyContainer;
            var ex = Assert.Throws<ComponentNotRegisteredException>(() => target.Resolve<object>());
            Assert.Contains("System.Object", ex.Message);
        }

        [Fact]
        public void WhenComponentIsRegistered_IsRegisteredReturnsTrueForAllServices()
        {
            var registration = Factory.CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)), new TypedService(typeof(string)) },
                Factory.CreateProvidedInstanceActivator("Hello"));

            var builder = Factory.CreateEmptyComponentRegistryBuilder();
            builder.Register(registration);
            var registry = builder.Build();
            var target = new Container(registry);

            Assert.True(target.IsRegistered<object>());
            Assert.True(target.IsRegistered<string>());
        }

        [Fact]
        public void WhenServiceIsRegistered_ResolveOptionalReturnsAnInstance()
        {
            var builder = Factory.CreateEmptyComponentRegistryBuilder();
            builder.Register(Factory.CreateSingletonRegistration(
                new[] { new TypedService(typeof(string)) },
                new ProvidedInstanceActivator("Hello")));
            var registry = builder.Build();

            var target = new Container(registry);

            var inst = target.ResolveOptional<string>();

            Assert.Equal("Hello", inst);
        }

        [Fact]
        public void WhenServiceNotRegistered_ResolveOptionalReturnsNull()
        {
            var target = new Container(Factory.CreateEmptyComponentRegistry());
            var inst = target.ResolveOptional<string>();
            Assert.Null(inst);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(param1, result.A);
            Assert.Equal(param2, result.B);
        }

        [Fact]
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

            Assert.Equal(a, result.A);
            Assert.Equal(b, result.B);
        }
    }
}
