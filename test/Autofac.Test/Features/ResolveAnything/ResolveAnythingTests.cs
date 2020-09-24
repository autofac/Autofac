// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Autofac.Features.ResolveAnything;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Features.ResolveAnything
{
    public class ResolveAnythingTests
    {
        public interface IInterfaceType
        {
        }

        [Fact]
        public void AConcreteTypeNotRegisteredWithTheContainerWillBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.True(container.IsRegistered<NotRegisteredType>());
        }

        [Fact]
        public void AllConcreteTypesSourceAlreadyRegisteredResolvesOptionalParams()
        {
            var cb = new ContainerBuilder();

            // Concrete type is already registered, but still errors
            cb.RegisterType<RegisterTypeWithCtorParam>();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            var resolved = container.Resolve<RegisterTypeWithCtorParam>();

            Assert.Equal("MyString", resolved.StringParam);
        }

        [Fact]
        public void AllConcreteTypesSourceResolvesOptionalParams()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            var resolved = container.Resolve<RegisterTypeWithCtorParam>();

            Assert.Equal("MyString", resolved.StringParam);
        }

        [Fact]
        public void AServiceAlreadyRegisteredWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterType<RegisteredType>();
            var container = cb.Build();
            Assert.True(container.IsRegistered<RegisteredType>());
            Assert.Single(container.Resolve<IEnumerable<object>>());
        }

        [Fact]
        public void AServiceProvideByAnotherRegistrationSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            cb.RegisterSource(new ObjectRegistrationSource());
            var container = cb.Build();
            Assert.True(container.IsRegistered<object>());
            Assert.Single(container.Resolve<IEnumerable<object>>());
        }

        [Fact]
        public void AServiceRegisteredBeforeTheSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<RegisteredType>();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            Assert.True(container.IsRegistered<RegisteredType>());
            Assert.Single(container.Resolve<IEnumerable<object>>());
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(IInterfaceType))]
        [InlineData(typeof(AbstractType))]
        [InlineData(typeof(Delegate))]
        [InlineData(typeof(MulticastDelegate))]
        [InlineData(typeof(Tuple<>))]
        public void IgnoresTypesThatShouldNotBeProvided(Type serviceType)
        {
            var source = new AnyConcreteTypeNotAlreadyRegisteredSource();
            var service = new TypedService(serviceType);
            Assert.False(source.RegistrationsFor(service, s => Enumerable.Empty<ServiceRegistration>()).Any(), $"Failed: {serviceType}");
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

        [Fact]
        public void TypesFromTheRegistrationSourceAreProvidedToOtherSources()
        {
            var container = CreateResolveAnythingContainer();

            // The RS for Func<> is getting the NotRegisteredType from the resolve-anything source
            Assert.True(container.IsRegistered<Func<NotRegisteredType>>());
            Assert.Single(container.Resolve<IEnumerable<Func<NotRegisteredType>>>());
            Assert.True(container.IsRegistered<Owned<NotRegisteredType>>());
            Assert.True(container.IsRegistered<Meta<NotRegisteredType>>());
            Assert.True(container.IsRegistered<Lazy<NotRegisteredType>>());
        }

        [Theory]
        [InlineData(typeof(Func<IInterfaceType>))]
        [InlineData(typeof(Owned<IInterfaceType>))]
        [InlineData(typeof(Meta<IInterfaceType>))]
        [InlineData(typeof(Lazy<IInterfaceType>))]
        public void IgnoredTypesFromTheRegistrationSourceAreNotProvidedToOtherSources(Type serviceType)
        {
            // Issue #495: Meta<T> not correctly handled with ACTNARS.
            var container = CreateResolveAnythingContainer();
            Assert.False(container.IsRegistered(serviceType), $"Failed: {serviceType}");
        }

        [Fact]
        public void DoesNotInterfereWithMetadata()
        {
            // Issue #495: Meta<T> not correctly handled with ACTNARS.
            var cb = new ContainerBuilder();
            cb.RegisterType<RegisteredType>().WithMetadata("value", 1);
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();

            var regType = container.Resolve<Meta<RegisteredType>>();
            Assert.Equal(1, regType.Metadata["value"]);

            var nonRegType = container.Resolve<Meta<NotRegisteredType>>();
            Assert.False(nonRegType.Metadata.ContainsKey("value"));

            var interfaceMeta = container.Resolve<IEnumerable<Meta<IInterfaceType>>>();
            Assert.Empty(interfaceMeta);

            var classMeta = container.Resolve<IEnumerable<Meta<NotRegisteredType>>>();
            Assert.Single(classMeta);
        }

        [Fact]
        public void ConstructableClosedGenericsCanBeResolved()
        {
            var container = CreateResolveAnythingContainer();
            Assert.True(container.IsRegistered<Tuple<Exception>>());
            Assert.NotNull(container.Resolve<Tuple<Exception>>());
        }

        [Fact]
        public void ConstructableOpenGenericsCanBeResolved()
        {
            var container = CreateResolveAnythingContainer();
            Assert.True(container.IsRegistered<Progress<Exception>>());
            Assert.NotNull(container.Resolve<Progress<Exception>>());
        }

        [Fact]
        public void ConstructableOpenGenericsWithUnresolvableTypeParametersCanBeResolved()
        {
            var container = CreateResolveAnythingContainer();
            Assert.True(container.IsRegistered<Progress<IComparable>>());
            Assert.NotNull(container.Resolve<Progress<IComparable>>());
            Assert.True(container.IsRegistered<Progress<AbstractType>>());
            Assert.NotNull(container.Resolve<Progress<AbstractType>>());
        }

        [Fact]
        public void ConstructableOpenGenericsWithGenericTypeArgumentNotMatchingFilterCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(t => t.Name.StartsWith("Progress")));
            var container = cb.Build();
            Assert.True(container.IsRegistered<Progress<Exception>>());
            Assert.NotNull(container.Resolve<Progress<Exception>>());
        }

        [Fact]
        public void WorksWithOpenGenericClassRegistrations()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(Progress<>)).AsSelf();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();
            Assert.True(container.IsRegistered<Progress<Exception>>());
            Assert.NotNull(container.Resolve<Progress<Exception>>());
        }

        [Fact]
        public void WorksWithOpenGenericInterfaceRegistrations()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(Progress<>)).As(typeof(IProgress<>));
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = cb.Build();
            Assert.True(container.IsRegistered<IProgress<Exception>>());
            Assert.NotNull(container.Resolve<IProgress<Exception>>());
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

        private static IContainer CreateResolveAnythingContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            return cb.Build();
        }

        public abstract class AbstractType
        {
        }

        public class NotRegisteredType
        {
        }

        public class RegisteredType
        {
        }

        public class RegisterTypeWithCtorParam
        {
            public RegisterTypeWithCtorParam(string stringParam = "MyString")
            {
                StringParam = stringParam;
            }

            public string StringParam { get; }
        }
    }
}
