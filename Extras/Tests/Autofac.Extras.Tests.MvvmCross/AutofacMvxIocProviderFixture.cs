using System;
using NUnit.Framework;
using Autofac.Extras.MvvmCross;
using Autofac.Core.Registration;
using Autofac.Core;

namespace Autofac.Extras.Tests.MvvmCross
{
    [TestFixture]
    public class AutofacMvxIocProviderFixture
    {
        private IContainer container;
        private AutofacMvxIocProvider provider;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
            provider = new AutofacMvxIocProvider(container);
        }

        [TearDown]
        public void TearDown()
        {
            provider.Dispose();
        }

        [Test]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(container);

            Assert.That(provider.CanResolve<object>(), Is.True);
        }

        [Test]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            Assert.That(provider.CanResolve<object>(), Is.False);
        }

        [Test]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(container);

            Assert.That(provider.Resolve<object>(), Is.TypeOf<object>());
            Assert.That(provider.Create<object>(), Is.TypeOf<object>());
            Assert.That(provider.IoCConstruct<object>(), Is.TypeOf<object>());
        }

        [Test]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.That(() => provider.Resolve<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
            Assert.That(() => provider.Create<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
            Assert.That(() => provider.IoCConstruct<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
        }

        [Test]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).SingleInstance();
            builder.Update(container);

            Assert.That(provider.GetSingleton<object>(), Is.TypeOf<object>());
            Assert.That(provider.GetSingleton<object>(), Is.SameAs(provider.GetSingleton<object>()));
        }

        [Test]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(container);

            Assert.That(() => provider.GetSingleton<object>(), Throws.TypeOf<DependencyResolutionException>());
        }

        [Test]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.That(() => provider.GetSingleton<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
        }

        [Test]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(container);

            object foo = null;
            var success = false;

            success = provider.TryResolve<object>(out foo);
            Assert.That(foo, Is.TypeOf<object>());
            Assert.That(success, Is.True);
        }

        [Test]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType<Interface,Concrete>();
            Assert.That(provider.Resolve<Interface>(), Is.TypeOf<Concrete>());
        }

        [Test]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var concreteViaFunc = new Concrete();
            provider.RegisterSingleton<Interface>(() => concreteViaFunc);
            Assert.That(provider.Resolve<Interface>(), Is.EqualTo(concreteViaFunc));
            Assert.That(provider.Resolve<Interface>(), Is.SameAs(provider.Resolve<Interface>()));

            var concreteInstance = new Concrete();
            provider.RegisterSingleton<Interface>(concreteInstance);
            Assert.That(provider.Resolve<Interface>(), Is.EqualTo(concreteInstance));
            Assert.That(provider.Resolve<Interface>(), Is.SameAs(provider.Resolve<Interface>()));
        }

        [Test]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            provider.CallbackWhenRegistered<Interface>(() => called = true);

            provider.RegisterType<Interface,Concrete>();
            Assert.That(called, Is.True);
        }

        private interface Interface
        {
        }

        private class Concrete : Interface
        {
        }
    }
}
