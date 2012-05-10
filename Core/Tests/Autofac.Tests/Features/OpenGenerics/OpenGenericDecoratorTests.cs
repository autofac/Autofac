using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Autofac.Tests.Features.OpenGenerics
{
    // ReSharper disable UnusedTypeParameter
    public interface IService<T>
    {
        IService<T> Decorated { get; }
    }
    // ReSharper restore UnusedTypeParameter

    public class ImplementorA<T> : IService<T>
    {
        public IService<T> Decorated
        {
            get { return this; }
        }
    }

    public class ImplementorB<T> : IService<T> 
    {
        public IService<T> Decorated
        {
            get { return this; }
        }
    }

    public class StringImplementor : IService<string>
    {
        public IService<string> Decorated
        {
            get { return this; }
        }
    }

    public abstract class Decorator<T> : IService<T>
    {
        readonly IService<T> _decorated;

        protected Decorator(IService<T> decorated)
        {
            _decorated = decorated;
        }

        public IService<T> Decorated
        {
            get { return _decorated; }
        }
    }

    public class DecoratorA<T> : Decorator<T>
    {
        public DecoratorA(IService<T> decorated) : base(decorated) { }
    }

    public class DecoratorB<T> : Decorator<T>
    {
        readonly string _parameter;

        public DecoratorB(IService<T> decorated, string parameter) : base(decorated)
        {
            _parameter = parameter;
        }

        public string Parameter
        {
            get { return _parameter; }
        }
    }

    [TestFixture]
    public class OpenGenericDecoratorTests
    {
        const string ParameterValue = "Abc";

        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            // Order is:
            //    A -> B(p) -> ImplementorA
            //    A -> B(p) -> ImplementorB
            //    A -> B(p) -> StringImplementor (string only)

            var builder = new ContainerBuilder();

            builder.RegisterType<StringImplementor>()
                .Named<IService<string>>("implementor");

            builder.RegisterGeneric(typeof(ImplementorA<>))
                .Named("implementor", typeof(IService<>));

            builder.RegisterGeneric(typeof(ImplementorB<>))
                .Named("implementor", typeof(IService<>));

            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IService<>), fromKey: "implementor", toKey: "b")
                .WithParameter("parameter", ParameterValue);

            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IService<>), fromKey: "b");

            _container = builder.Build();
        }

        [Test]
        public void CanResolveDecoratorService()
        {
            Assert.IsNotNull(_container.Resolve<IService<int>>());
        }

        [Test]
        public void ThereAreTwoImplementorsOfInt()
        {
            Assert.AreEqual(2, _container.ResolveNamed<IEnumerable<IService<int>>>("implementor").Count());
        }

        [Test]
        public void ThereAreTwoBLevelDecoratorsOfInt()
        {
            Assert.AreEqual(2, _container.ResolveNamed<IEnumerable<IService<int>>>("b").Count());
        }

        [Test]
        public void TheDefaultImplementorIsTheLastRegistered()
        {
            var defaultChain = _container.Resolve<IService<int>>();
            Assert.IsInstanceOf<ImplementorB<int>>(defaultChain.Decorated.Decorated);
        }

        [Test]
        public void AllGenericImplemetationsAreDecorated()
        {
            Assert.AreEqual(2, _container.Resolve<IEnumerable<IService<int>>>().Count());
        }

        [Test]
        public void WhenClosedImplementationsAreAvailableTheyAreDecorated()
        {
            Assert.AreEqual(3, _container.Resolve<IEnumerable<IService<string>>>().Count());
        }

        [Test]
        public void TheFirstDecoratorIsA()
        {
            Assert.IsInstanceOf<DecoratorA<int>>(_container.Resolve<IService<int>>());
        }

        [Test]
        public void TheSecondDecoratorIsB()
        {
            Assert.IsInstanceOf<DecoratorB<int>>(_container.Resolve<IService<int>>().Decorated);
        }

        [Test]
        public void ParametersArePassedToB()
        {
            Assert.AreEqual(ParameterValue, ((DecoratorB<int>) _container.Resolve<IService<int>>().Decorated).Parameter);
        }
    }
}
