using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.ResolveAnything;
using Autofac.Tests.Scenarios.RegistrationSources;
using NUnit.Framework;

namespace Autofac.Tests.Features.ResolveAnything
{
    [TestFixture]
    public class ResolveAnythingTests
    {
        public class NotRegisteredType { }

        [Test]
        public void AConcreteTypeNotRegisteredWithTheContainerWillBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.That(container.IsRegistered<NotRegisteredType>());
        }

        public abstract class AbstractType { }

        [Test]
        public void AnAbstractTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.IsFalse(container.IsRegistered<AbstractType>());
        }

        public interface IInterfaceType { }

        [Test]
        public void AnInterfaceTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.IsFalse(container.IsRegistered<IInterfaceType>());
        }

        [Test]
        public void TypesFromTheRegistrationSourceAreProvidedToOtherSources()
        {
            var container = CreateResolveAnythingContainer();
            // The RS for Func<> is getting the NotRegisteredType from the resolve-anything source
            Assert.That(container.IsRegistered<Func<NotRegisteredType>>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<Func<NotRegisteredType>>>().Count());
        }

        [Test]
        public void AServiceProvideByAnotherRegistrationSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterSource(new ObjectRegistrationSource());
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        public class RegisteredType { }

        [Test]
        public void AServiceAlreadyRegisteredWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterType<RegisteredType>();
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<RegisteredType>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        [Test]
        public void AServiceRegisteredBeforeTheSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<RegisteredType>();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            Assert.IsTrue(container.IsRegistered<RegisteredType>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        [Test]
        public void TypesIgnoredUsingPredicateAreNotResolvedFromTheContainer()
        {
            var cb = new ContainerBuilder();
            var registrationSource = new AnyConcreteTypeNotAlreadyRegisteredSource(t => !t.IsAssignableTo<string>());
            cb.RegisterSource(registrationSource);
            var container = cb.Build();
            Assert.That(container.IsRegistered<string>(), Is.False);
        }

        [Test]
        public void LifetimeCanBeInstancePerLifetimeScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(rb => rb.InstancePerLifetimeScope()));
            var container = cb.Build();

            var first = container.Resolve<NotRegisteredType>();
            var second = container.Resolve<NotRegisteredType>();
            Assert.AreSame(first, second);
            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<NotRegisteredType>();
                Assert.AreNotSame(first, third);
                var fourth = scope.Resolve<NotRegisteredType>();
                Assert.AreSame(third, fourth);
            }
        }

        [Test]
        public void LifetimeCanBeSingleInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(rb => rb.SingleInstance()));
            var container = cb.Build();

            var first = container.Resolve<NotRegisteredType>();
            var second = container.Resolve<NotRegisteredType>();
            Assert.AreSame(first, second);
            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<NotRegisteredType>();
                Assert.AreSame(first, third);
            }
        }

        static IContainer CreateResolveAnythingContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            return cb.Build();
        }

    }
}
