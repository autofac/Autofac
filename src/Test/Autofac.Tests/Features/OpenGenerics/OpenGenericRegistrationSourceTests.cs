using System;
using Autofac.Tests.Util;
using NUnit.Framework;
using System.Linq;
using Autofac.Core;
using Autofac.Features.OpenGenerics;
using Autofac.Builder;

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class OpenGenericRegistrationSourceTests
    {
        // ReSharper disable UnusedTypeParameter
        public interface I<T> { }

        public class A1<T> : DisposeTracker, I<T> { }

        [Test]
        public void GeneratesActivatorAndCorrectServices()
        {
            var g = ConstructSource(typeof (A1<>), typeof (I<>));

            var r = g
                .RegistrationsFor(new TypedService(typeof(I<int>)), s => null)
                .Single();

            Assert.AreEqual(typeof(I<int>),
                r.Services.Cast<TypedService>().Single().ServiceType);

            var activatedInstance = r.Activator.ActivateInstance(Container.Empty, Factory.NoParameters);
            Assert.IsInstanceOf<A1<int>>(activatedInstance);
        }

        public class AWithNew<T> : I<T>
            where T : new()
        {
        }

        [Test]
        [IgnoreOnPhone("Reflection does not provide information to check")]
        public void DoesNotGenerateActivatorWhenConstructorConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivatorForI<string>(typeof(AWithNew<>)));
        }

        public class PWithNew { }

        [Test]
        public void GeneratesActivatorWhenConstructorConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivatorForI<PWithNew>(typeof(AWithNew<>)));
        }

        public class AWithDisposable<T> : I<T>
            where T : IDisposable
        {
        }

        [Test]
        [IgnoreOnPhone("Reflection does not provide information to check")]
        public void DoesNotGenerateActivatorWhenTypeConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivatorForI<string>(typeof(AWithDisposable<>)));
        }

        [Test]
        public void GeneratesActivatorWhenTypeConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivatorForI<DisposeTracker>(typeof(AWithDisposable<>)));
        }

        public class AWithClass<T> : I<T>
            where T : class { }

        [Test]
        [IgnoreOnPhone("Reflection does not provide information to check")]
        public void DoesNotGenerateActivatorWhenClassConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivatorForI<int>(typeof(AWithClass<>)));
        }

        [Test]
        public void GeneratesActivatorWhenClassConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivatorForI<string>(typeof(AWithClass<>)));
        }

        public class AWithValue<T> : I<T>
            where T : struct { }

        [Test]
        [IgnoreOnPhone("Reflection does not provide information to check")]
        public void DoesNotGenerateActivatorWhenValueConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivatorForI<string>(typeof(AWithValue<>)));
        }

        [Test]
        public void GeneratesActivatorWhenValueConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivatorForI<int>(typeof(AWithValue<>)));
        }

        static bool CanGenerateActivatorForI<TClosing>(Type implementor)
        {
            var g = ConstructSource(implementor, typeof (I<>));

            var rs = g.RegistrationsFor(new TypedService(typeof(I<TClosing>)), s => null);

            return rs.Count() == 1;
        }

        public interface ITwoParams<T, U> { }
        public class TwoParams<T, U> : ITwoParams<T, U> { }

        [Test]
        public void SupportsMultipleGenericParameters()
        {
            var g = ConstructSource(typeof(TwoParams<,>));

            var rs = g.RegistrationsFor(new TypedService(typeof(TwoParams<int, string>)), s => null);

            Assert.AreEqual(1, rs.Count());
        }

        [Test]
        public void SupportsMultipleGenericParametersMappedFromService()
        {
            var g = ConstructSource(typeof (TwoParams<,>), typeof (ITwoParams<,>));

            var rs = g.RegistrationsFor(new TypedService(typeof(ITwoParams<int, string>)), s => null);

            Assert.AreEqual(1, rs.Count());
        }

        public interface IEntity<TId> { }

        public class EntityOfInt : IEntity<int> { }

        public class Repository<T, TId> where T : IEntity<TId> { }

        [Test]
        public void SupportsCodependentTypeConstraints()
        {
            var g = ConstructSource(typeof(Repository<,>));

            var rs = g.RegistrationsFor(new TypedService(typeof(Repository<EntityOfInt, int>)), s => null);

            Assert.AreEqual(1, rs.Count());
        }

        public interface IHaveNoParameters { }

        public interface IHaveOneParameter<T> { }

        public interface IHaveTwoParameters<T,U> { }

        public interface IHaveThreeParameters<T,U,V> { }

        public class HaveTwoParameters<T,U> : IHaveThreeParameters<T,U,U>, IHaveTwoParameters<T,T>, IHaveOneParameter<T>, IHaveNoParameters { }

        public interface IUnrelated { }

        [Test]
        public void RejectsServicesWithoutTypeParameters()
        {
            Assert.Throws<ArgumentException>(() => ConstructSource(typeof(HaveTwoParameters<,>), typeof(IHaveNoParameters)));
        }

        [Test]
        public void RejectsServicesNotInTheInheritanceChain()
        {
            Assert.Throws<ArgumentException>(() => ConstructSource(typeof(HaveTwoParameters<,>), typeof(IUnrelated)));
        }

        [Test]
        public void IgnoresServicesWithoutEnoughParameters()
        {
            Assert.That(!SourceCanSupply<IHaveOneParameter<int>>(typeof(HaveTwoParameters<,>)));
        }

        [Test]
        [IgnoreOnPhone("Fails because IsCompatibleWithGenericParameterConstraints cannot check contraints")]
        public void IgnoresServicesThatDoNotSupplyAllParameters()
        {
            Assert.That(!SourceCanSupply<IHaveTwoParameters<int,int>>(typeof(HaveTwoParameters<,>)));
        }

        [Test]
        public void AcceptsServicesWithMoreParametersWhenAllImplementationParametersCovered()
        {
            Assert.That(SourceCanSupply<IHaveThreeParameters<int,string,string>>(typeof(HaveTwoParameters<,>)));
        }

        [Test]
        public void IgnoresServicesWithMismatchedParameters()
        {
            Assert.That(!SourceCanSupply<IHaveThreeParameters<int,string,decimal>>(typeof(HaveTwoParameters<,>)));
        }

        static bool SourceCanSupply<TClosedService>(Type component)
        {
            var service = typeof (TClosedService).GetGenericTypeDefinition();
            var source = ConstructSource(component, service);

            var closedServiceType = typeof(TClosedService);
            var registrations = source.RegistrationsFor(new TypedService(closedServiceType), s => Enumerable.Empty<IComponentRegistration>());
            if (registrations.Count() != 1)
                return false;

            var registration = registrations.Single();
            var instance = registration.Activator.ActivateInstance(Container.Empty, Factory.NoParameters);

            Assert.That(closedServiceType.IsAssignableFrom(instance.GetType()));
            return true;
        }

        static OpenGenericRegistrationSource ConstructSource(Type component, Type service = null)
        {
            return new OpenGenericRegistrationSource(
                new RegistrationData(new TypedService(service ?? component)),
                new ReflectionActivatorData(component));
        }
        // ReSharper restore UnusedTypeParameter
    }
}
