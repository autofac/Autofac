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
        IContainer _container;
        AutofacMvxIocProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _container = new ContainerBuilder().Build();
            _provider = new AutofacMvxIocProvider(_container);
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.That(_provider.CanResolve<object>(), Is.True);
        }

        [Test]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            Assert.That(_provider.CanResolve<object>(), Is.False);
        }

        [Test]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => _provider.CanResolve(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.That(_provider.Resolve<object>(), Is.TypeOf<object>());
            Assert.That(_provider.Create<object>(), Is.TypeOf<object>());
            Assert.That(_provider.IoCConstruct<object>(), Is.TypeOf<object>());
        }

        [Test]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.That(() => _provider.Resolve<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
            Assert.That(() => _provider.Create<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
            Assert.That(() => _provider.IoCConstruct<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
        }

        [Test]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => _provider.Resolve(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.Create(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.IoCConstruct(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).SingleInstance();
            builder.Update(_container);

            Assert.That(_provider.GetSingleton<object>(), Is.TypeOf<object>());
            Assert.That(_provider.GetSingleton<object>(), Is.SameAs(_provider.GetSingleton<object>()));
        }

        [Test]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            Assert.That(() => _provider.GetSingleton<object>(), Throws.TypeOf<DependencyResolutionException>());
        }

        [Test]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.That(() => _provider.GetSingleton<object>(), Throws.TypeOf<ComponentNotRegisteredException>());
        }

        [Test]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.That(() => _provider.GetSingleton(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Update(_container);

            object foo;
            var success = _provider.TryResolve(out foo);

            Assert.That(foo, Is.TypeOf<object>());
            Assert.That(success, Is.True);
        }

        [Test]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType<IInterface, Concrete>();
            var instance = _provider.Resolve<IInterface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(_provider.Resolve<IInterface>()));
        }

        [Test]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType<IInterface>(() => new Concrete());
            var instance = _provider.Resolve<IInterface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(_provider.Resolve<IInterface>()));

            _provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.That(_provider.Resolve<IInterface>(), Is.Not.SameAs(_provider.Resolve<IInterface>()));
        }

        [Test]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            _provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = _provider.Resolve<IInterface>();
            Assert.That(instance, Is.TypeOf<Concrete>());
            Assert.That(instance, Is.Not.SameAs(_provider.Resolve<IInterface>()));
        }

        [Test]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            Assert.That(() => _provider.RegisterType(null, typeof(object)), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterType(typeof(object), (Type)null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.That(() => _provider.RegisterType((Func<object>)null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterType(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterType(typeof(object), (Func<object>)null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var concreteViaFunc = new Concrete();
            _provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.That(_provider.Resolve<IInterface>(), Is.EqualTo(concreteViaFunc));
            Assert.That(_provider.Resolve<IInterface>(), Is.SameAs(_provider.Resolve<IInterface>()));

            var concreteInstance = new Concrete();
            _provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.That(_provider.Resolve<IInterface>(), Is.EqualTo(concreteInstance));
            Assert.That(_provider.Resolve<IInterface>(), Is.SameAs(_provider.Resolve<IInterface>()));
        }

        [Test]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.That(() => _provider.RegisterSingleton((IInterface)null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterSingleton((Func<IInterface>)null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterSingleton(null, new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterSingleton(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.RegisterSingleton(typeof(object), null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            _provider.CallbackWhenRegistered<IInterface>(() => called = true);

            _provider.RegisterType<IInterface, Concrete>();
            Assert.That(called, Is.True);
        }

        [Test]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            Assert.That(() => _provider.CallbackWhenRegistered(null, () => new object()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _provider.CallbackWhenRegistered(typeof(object), null), Throws.TypeOf<ArgumentNullException>());
        }

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
        }
    }
}
