using System;
using Autofac.Extras.MvvmCross;
using Autofac.Core.Registration;
using Autofac.Core;
using NUnit.Framework;

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
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => provider.CanResolve(null), Throws.TypeOf<ArgumentNullException>());
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
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => provider.Resolve(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.Create(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.IoCConstruct(null), Throws.TypeOf<ArgumentNullException>());
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
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => provider.GetSingleton(null), Throws.TypeOf<ArgumentNullException>());
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
            provider.RegisterType<Interface, Concrete>();
            var instance = provider.Resolve<Interface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(provider.Resolve<Interface>()));
        }

        [Test]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType<Interface>(() => new Concrete());
            var instance = provider.Resolve<Interface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(provider.Resolve<Interface>()));

            provider.RegisterType(typeof(Interface), () => new Concrete());
            Assert.That(provider.Resolve<Interface>(), Is.Not.SameAs(provider.Resolve<Interface>()));
        }

        [Test]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType(typeof(Interface), () => new Concrete());
            var instance = provider.Resolve<Interface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(provider.Resolve<Interface>()));
        }

        [Test]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            Assert.That(() => provider.RegisterType(null, typeof(object)), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterType(typeof(object), (Type)null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.That(() => provider.RegisterType((Func<object>)null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterType(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterType(typeof(object), (Func<object>)null), Throws.TypeOf<ArgumentNullException>());
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
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Interface nullInterface = null;
            Func<Interface> nullConstructor = null;
            Assert.That(() => provider.RegisterSingleton<Interface>(nullInterface), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterSingleton<Interface>(nullConstructor), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterSingleton(null, new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterSingleton(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterSingleton(typeof(object), null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.RegisterSingleton(typeof(object), nullConstructor), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            provider.CallbackWhenRegistered<Interface>(() => called = true);

            provider.RegisterType<Interface,Concrete>();
            Assert.That(called, Is.True);
        }

        [Test]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            Assert.That(() => provider.CallbackWhenRegistered(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => provider.CallbackWhenRegistered(typeof(object), null), Throws.TypeOf<ArgumentNullException>());
        }

        private interface Interface
        {
        }

        private class Concrete : Interface
        {
        }
    }
}
