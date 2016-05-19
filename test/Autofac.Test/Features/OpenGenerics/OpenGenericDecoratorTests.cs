using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Test.Features.OpenGenerics
{
    public class OpenGenericDecoratorTests
    {
        public interface IService<T>
        {
            IService<T> Decorated { get; }
        }

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
            private readonly IService<T> _decorated;

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
            public DecoratorA(IService<T> decorated)
                : base(decorated)
            {
            }
        }

        public class DecoratorB<T> : Decorator<T>
        {
            private readonly string _parameter;

            public DecoratorB(IService<T> decorated, string parameter)
                : base(decorated)
            {
                _parameter = parameter;
            }

            public string Parameter
            {
                get { return _parameter; }
            }
        }

        private const string ParameterValue = "Abc";

        private IContainer _container;

        public OpenGenericDecoratorTests()
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

        [Fact]
        public void CanResolveDecoratorService()
        {
            Assert.NotNull(_container.Resolve<IService<int>>());
        }

        [Fact]
        public void ThereAreTwoImplementorsOfInt()
        {
            Assert.Equal(2, _container.ResolveNamed<IEnumerable<IService<int>>>("implementor").Count());
        }

        [Fact]
        public void ThereAreTwoBLevelDecoratorsOfInt()
        {
            Assert.Equal(2, _container.ResolveNamed<IEnumerable<IService<int>>>("b").Count());
        }

        [Fact]
        public void TheDefaultImplementorIsTheLastRegistered()
        {
            var defaultChain = _container.Resolve<IService<int>>();
            Assert.IsType<ImplementorB<int>>(defaultChain.Decorated.Decorated);
        }

        [Fact]
        public void AllGenericImplemetationsAreDecorated()
        {
            Assert.Equal(2, _container.Resolve<IEnumerable<IService<int>>>().Count());
        }

        [Fact]
        public void WhenClosedImplementationsAreAvailableTheyAreDecorated()
        {
            Assert.Equal(3, _container.Resolve<IEnumerable<IService<string>>>().Count());
        }

        [Fact]
        public void TheFirstDecoratorIsA()
        {
            Assert.IsType<DecoratorA<int>>(_container.Resolve<IService<int>>());
        }

        [Fact]
        public void TheSecondDecoratorIsB()
        {
            Assert.IsType<DecoratorB<int>>(_container.Resolve<IService<int>>().Decorated);
        }

        [Fact]
        public void ParametersArePassedToB()
        {
            Assert.Equal(ParameterValue, ((DecoratorB<int>)_container.Resolve<IService<int>>().Decorated).Parameter);
        }
    }
}
