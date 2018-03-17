using System.Linq;
using Autofac.Core;
using Autofac.Features.Decorators;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class OpenGenericDecoratorTests
    {
        public interface IService<T>
        {
            IService<T> Decorated { get; }
        }

        public class ImplementorA<T> : IService<T>
        {
            public IService<T> Decorated => this;
        }

        public class ImplementorB<T> : IService<T>
        {
            public IService<T> Decorated => this;
        }

        public class StringImplementor : IService<string>
        {
            public IService<string> Decorated => this;
        }

        public abstract class Decorator<T> : IService<T>
        {
            protected Decorator(IService<T> decorated)
            {
                Decorated = decorated;
            }

            public IService<T> Decorated { get; }
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
            public DecoratorB(IService<T> decorated)
                : base(decorated)
            {
            }
        }

        public interface IDecoratorWithParameter
        {
            string Parameter { get; }
        }

        public class DecoratorWithParameter<T> : Decorator<T>, IDecoratorWithParameter
        {
            public DecoratorWithParameter(IService<T> decorated, string parameter)
                : base(decorated)
            {
                Parameter = parameter;
            }

            public string Parameter { get; }
        }

        [Fact]
        public void CanDecorateOpenGenericType()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IService<>));
            var container = builder.Build();

            var instance = container.Resolve<IService<int>>();

            Assert.IsType<DecoratorA<int>>(instance);
            Assert.IsType<ImplementorA<int>>(instance.Decorated);
        }

        [Fact]
        public void CanDecorateMultipleServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IService<>));
            var container = builder.Build();

            var instance = container.Resolve<IService<int>>();

            Assert.IsType<DecoratorB<int>>(instance);
            Assert.IsType<DecoratorA<int>>(instance.Decorated);
            Assert.IsType<ImplementorA<int>>(instance.Decorated.Decorated);
        }
    }
}
