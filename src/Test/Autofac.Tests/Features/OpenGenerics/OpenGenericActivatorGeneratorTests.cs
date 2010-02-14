using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Autofac.Core;
using Autofac.Features.OpenGenerics;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class OpenGenericActivatorGeneratorTests
    {
        interface I<T> { }

        class A1<T> : DisposeTracker, I<T> { }

        [Test]
        public void GeneratesActivatorAndCorrectServices()
        {
            var g = new OpenGenericActivatorGenerator();

            IInstanceActivator activator;
            IEnumerable<Service> services;

            Assert.IsTrue(
                g.TryGenerateActivator(new TypedService(typeof(I<int>)),
                    new Service[] { new TypedService(typeof(I<>)) },
                    new ReflectionActivatorData(typeof(A1<>)),
                    out activator, out services));

            Assert.AreEqual(typeof(I<int>),
                services.Cast<TypedService>().Single().ServiceType);

            var activatedInstance = activator.ActivateInstance(Container.Empty, Factory.NoParameters);
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
            var g = new OpenGenericActivatorGenerator();

            IInstanceActivator activator;
            IEnumerable<Service> services;

            return g.TryGenerateActivator(new TypedService(typeof(I<TClosing>)),
                    new Service[] { new TypedService(typeof(I<>)) },
                    new ReflectionActivatorData(implementor),
                    out activator, out services);
        }
    }
}
