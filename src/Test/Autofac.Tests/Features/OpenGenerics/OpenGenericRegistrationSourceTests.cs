using System;
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
        interface I<T> { }

        class A1<T> : DisposeTracker, I<T> { }

        [Test]
        public void GeneratesActivatorAndCorrectServices()
        {
            var g = new OpenGenericRegistrationSource(
                new RegistrationData(new TypedService(typeof(I<>))),
                new ReflectionActivatorData(typeof(A1<>)));

            var r = g
                .RegistrationsFor(new TypedService(typeof(I<int>)), s => null)
                .Single();

            Assert.AreEqual(typeof(I<int>),
                r.Services.Cast<TypedService>().Single().ServiceType);

            var activatedInstance = r.Activator.ActivateInstance(Container.Empty, Factory.NoParameters);
            Assert.IsInstanceOf<A1<int>>(activatedInstance);
        }

        class AWithNew<T> : I<T>
            where T : new()
        {
        }

        [Test]
        public void DoesNotGenerateActivatorWhenConstructorConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivator<string>(typeof(AWithNew<>)));
        }

        class PWithNew { }

        [Test]
        public void GeneratesActivatorWhenConstructorConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivator<PWithNew>(typeof(AWithNew<>)));
        }

        class AWithDisposable<T> : I<T>
            where T : IDisposable
        {
        }

        [Test]
        public void DoesNotGenerateActivatorWhenTypeConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivator<string>(typeof(AWithDisposable<>)));
        }

        [Test]
        public void GeneratesActivatorWhenTypeConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivator<DisposeTracker>(typeof(AWithDisposable<>)));
        }

        class AWithClass<T> : I<T>
            where T : class { }

        [Test]
        public void DoesNotGenerateActivatorWhenClassConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivator<int>(typeof(AWithClass<>)));
        }

        [Test]
        public void GeneratesActivatorWhenClassConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivator<string>(typeof(AWithClass<>)));
        }

        class AWithValue<T> : I<T>
            where T : struct { }

        [Test]
        public void DoesNotGenerateActivatorWhenValueConstraintBroken()
        {
            Assert.IsFalse(CanGenerateActivator<string>(typeof(AWithValue<>)));
        }

        [Test]
        public void GeneratesActivatorWhenValueConstraintMet()
        {
            Assert.IsTrue(CanGenerateActivator<int>(typeof(AWithValue<>)));
        }

        static bool CanGenerateActivator<TClosing>(Type implementor)
        {
            var g = new OpenGenericRegistrationSource(
                new RegistrationData(new TypedService(typeof(I<>))),
                new ReflectionActivatorData(implementor));

            var rs = g.RegistrationsFor(new TypedService(typeof(I<TClosing>)), s => null);

            return rs.Count() == 1;
        }

        public interface ITwoParams<T, U> { }
        public class TwoParams<T, U> : ITwoParams<T, U> { }

        [Test]
        public void SupportsMultipleGenericParameters()
        {
            var tp = typeof(TwoParams<,>);

            var g = new OpenGenericRegistrationSource(
                new RegistrationData(new TypedService(tp)),
                new ReflectionActivatorData(tp));

            var rs = g.RegistrationsFor(new TypedService(typeof(TwoParams<int, string>)), s => null);

            Assert.AreEqual(1, rs.Count());
        }

        [Test]
        public void SupportsMultipleGenericParametersMappedFromService()
        {
            var g = new OpenGenericRegistrationSource(
                new RegistrationData(new TypedService(typeof(ITwoParams<,>))),
                new ReflectionActivatorData(typeof(TwoParams<,>)));

            var rs = g.RegistrationsFor(new TypedService(typeof(ITwoParams<int, string>)), s => null);

            Assert.AreEqual(1, rs.Count());
        }
    }
}
