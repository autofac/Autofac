using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.ResolveAnything;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Features.ResolveAnything
{
    public class ResolveAnythingTests
    {
        public class NotRegisteredType
        {
        }

        [Fact]
        public void AConcreteTypeNotRegisteredWithTheContainerWillBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.True(container.IsRegistered<NotRegisteredType>());
        }

        public abstract class AbstractType
        {
        }

        [Fact]
        public void AnAbstractTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.False(container.IsRegistered<AbstractType>());
        }

        public interface IInterfaceType
        {
        }

        [Fact]
        public void AnInterfaceTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.False(container.IsRegistered<IInterfaceType>());
        }

        [Fact]
        public void TypesFromTheRegistrationSourceAreProvidedToOtherSources()
        {
            var container = CreateResolveAnythingContainer();

            // The RS for Func<> is getting the NotRegisteredType from the resolve-anything source
            Assert.True(container.IsRegistered<Func<NotRegisteredType>>());
            Assert.Equal(1, container.Resolve<IEnumerable<Func<NotRegisteredType>>>().Count());
        }

        [Fact]
        public void AServiceProvideByAnotherRegistrationSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterSource(new ObjectRegistrationSource());
            var container = cb.Build();
            Assert.True(container.IsRegistered<object>());
            Assert.Equal(1, container.Resolve<IEnumerable<object>>().Count());
        }

        public class RegisteredType
        {
        }

        [Fact]
        public void AServiceAlreadyRegisteredWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterType<RegisteredType>();
            var container = cb.Build();
            Assert.True(container.IsRegistered<RegisteredType>());
            Assert.Equal(1, container.Resolve<IEnumerable<object>>().Count());
        }

        [Fact]
        public void AServiceRegisteredBeforeTheSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<RegisteredType>();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            Assert.True(container.IsRegistered<RegisteredType>());
            Assert.Equal(1, container.Resolve<IEnumerable<object>>().Count());
        }

        [Fact]
        public void TypesIgnoredUsingPredicateAreNotResolvedFromTheContainer()
        {
            var cb = new ContainerBuilder();
            var registrationSource = new AnyConcreteTypeNotAlreadyRegisteredSource(t => !t.IsAssignableTo<string>());
            cb.RegisterSource(registrationSource);
            var container = cb.Build();
            Assert.False(container.IsRegistered<string>());
        }

        [Fact]
        public void LifetimeCanBeInstancePerLifetimeScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(rb => rb.InstancePerLifetimeScope()));
            var container = cb.Build();

            var first = container.Resolve<NotRegisteredType>();
            var second = container.Resolve<NotRegisteredType>();
            Assert.Same(first, second);
            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<NotRegisteredType>();
                Assert.NotSame(first, third);
                var fourth = scope.Resolve<NotRegisteredType>();
                Assert.Same(third, fourth);
            }
        }

        [Fact]
        public void LifetimeCanBeSingleInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(rb => rb.SingleInstance()));
            var container = cb.Build();

            var first = container.Resolve<NotRegisteredType>();
            var second = container.Resolve<NotRegisteredType>();
            Assert.Same(first, second);
            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<NotRegisteredType>();
                Assert.Same(first, third);
            }
        }

        private static IContainer CreateResolveAnythingContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            return cb.Build();
        }
    }
}
