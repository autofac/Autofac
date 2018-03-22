using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Decorators;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class OpenGenericDecoratorTests
    {
        // ReSharper disable once UnusedTypeParameter
        public interface IService<T>
        {
        }

        public interface IDecoratedService<T> : IService<T>
        {
            IDecoratedService<T> Decorated { get; }
        }

        public class ImplementorA<T> : IDecoratedService<T>
        {
            public IDecoratedService<T> Decorated => this;
        }

        public class ImplementorB<T> : IDecoratedService<T>
        {
            public IDecoratedService<T> Decorated => this;
        }

        public class ImplementorWithParameters<T> : IDecoratedService<T>
        {
            public IDecoratedService<T> Decorated => this;

            public string Parameter { get; }

            public ImplementorWithParameters(string parameter)
            {
                Parameter = parameter;
            }
        }

        public abstract class Decorator<T> : IDecoratedService<T>
        {
            protected Decorator(IDecoratedService<T> decorated)
            {
                Decorated = decorated;
            }

            public IDecoratedService<T> Decorated { get; }
        }

        public class DecoratorA<T> : Decorator<T>
        {
            public DecoratorA(IDecoratedService<T> decorated)
                : base(decorated)
            {
            }
        }

        public class DecoratorB<T> : Decorator<T>
        {
            public DecoratorB(IDecoratedService<T> decorated)
                : base(decorated)
            {
            }
        }

        public class StringImplementor : Decorator<string>
        {
            public StringImplementor(IDecoratedService<string> decorated)
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
            public DecoratorWithParameter(IDecoratedService<T> decorated, string parameter)
                : base(decorated)
            {
                Parameter = parameter;
            }

            public string Parameter { get; }
        }

        public interface IDecoratorWithContext
        {
            IDecoratorContext Context { get; }
        }

        public class DecoratorWithContextA<T> : Decorator<T>, IDecoratorWithContext
        {
            public DecoratorWithContextA(IDecoratedService<T> decorated, IDecoratorContext context)
                : base(decorated)
            {
                Context = context;
            }

            public IDecoratorContext Context { get; }
        }

        public class DecoratorWithContextB<T> : Decorator<T>, IDecoratorWithContext
        {
            public DecoratorWithContextB(IDecoratedService<T> decorated, IDecoratorContext context)
                : base(decorated)
            {
                Context = context;
            }

            public IDecoratorContext Context { get; }
        }

        [Fact]
        public void RegistrationIncludesTheServiceType()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var registration = container.RegistrationFor<IDecoratedService<int>>();
            Assert.NotNull(registration);

            var decoratedService = new TypedService(typeof(IDecoratedService<int>));
            Assert.Contains(registration.Services.OfType<TypedService>(), s => s == decoratedService);
        }

        [Fact]
        public void RegistrationTargetsTheImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var registration = container.RegistrationFor<IDecoratedService<int>>();

            Assert.NotNull(registration);
            Assert.Equal(registration.Target.Activator.LimitType, typeof(ImplementorA<int>));
        }

        [Fact(Skip ="Cannot currently determine requested resolve service type")]
        public void DecoratedRegistrationCanIncludeImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).AsSelf();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            Assert.IsType<ImplementorA<int>>(container.Resolve<ImplementorA<int>>());
        }

        [Fact]
        public void DecoratedRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).As(typeof(IService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var serviceRegistration = container.RegistrationFor<IService<int>>();
            var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService<int>>();

            Assert.NotNull(serviceRegistration);
            Assert.NotNull(decoratedServiceRegistration);
            Assert.Same(serviceRegistration, decoratedServiceRegistration);

            Assert.IsType<DecoratorA<int>>(container.Resolve<IService<int>>());
            Assert.IsType<DecoratorA<int>>(container.Resolve<IDecoratedService<int>>());
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenNoDecoratorsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<ImplementorA<int>>(instance);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenSingleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(instance);
            Assert.IsType<ImplementorA<int>>(instance.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenMultipleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorB<int>>(instance);
            Assert.IsType<DecoratorA<int>>(instance.Decorated);
            Assert.IsType<ImplementorA<int>>(instance.Decorated.Decorated);
        }

        [Fact]
        public void CanResolveMultipleDecoratedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGeneric(typeof(ImplementorB<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService<int>>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorA<int>>(s);
                    Assert.IsType<ImplementorA<int>>(s.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorA<int>>(s);
                    Assert.IsType<ImplementorB<int>>(s.Decorated);
                });
        }

        [Fact]
        public void CanResolveDecoratorWithFunc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var factory = container.Resolve<Func<IDecoratedService<int>>>();

            Assert.IsType<DecoratorA<int>>(factory());
        }

        [Fact]
        public void CanResolveDecoratorWithLazy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var lazy = container.Resolve<Lazy<IDecoratedService<int>>>();

            Assert.IsType<DecoratorA<int>>(lazy.Value);
        }

        [Fact]
        public void DecoratorRegistrationsGetAppliedInOrderAdded()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorB<int>>(instance);
            Assert.IsType<DecoratorA<int>>(instance.Decorated);
            Assert.IsType<ImplementorA<int>>(instance.Decorated.Decorated);

            builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            container = builder.Build();

            instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(instance);
            Assert.IsType<DecoratorB<int>>(instance.Decorated);
            Assert.IsType<ImplementorA<int>>(instance.Decorated.Decorated);
        }

        [Fact]
        public void CanApplyDecoratorConditionallyAtRuntime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>), context => context.AppliedDecorators.Any());
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorB<int>>(instance);
            Assert.IsType<ImplementorA<int>>(instance.Decorated);
        }

        [Fact]
        public void CanInjectDecoratorContextAsSnapshot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorWithContextA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorWithContextA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            var contextB = ((IDecoratorWithContext)instance).Context;
            Assert.Equal(typeof(IDecoratedService<int>), contextB.ServiceType);
            Assert.Equal(typeof(ImplementorA<int>), contextB.ImplementationType);
            Assert.IsType<DecoratorWithContextA<int>>(contextB.CurrentInstance);
            Assert.Collection(
                contextB.AppliedDecorators,
                item => Assert.IsType<DecoratorA<int>>(item),
                item => Assert.IsType<DecoratorB<int>>(item),
                item => Assert.IsType<DecoratorWithContextA<int>>(item));
            Assert.Collection(
                contextB.AppliedDecoratorTypes,
                item => Assert.Equal(typeof(DecoratorA<int>), item),
                item => Assert.Equal(typeof(DecoratorB<int>), item),
                item => Assert.Equal(typeof(DecoratorWithContextA<int>), item));

            var contextA = ((IDecoratorWithContext)instance.Decorated).Context;
            Assert.Equal(typeof(IDecoratedService<int>), contextA.ServiceType);
            Assert.Equal(typeof(ImplementorA<int>), contextA.ImplementationType);
            Assert.IsType<DecoratorB<int>>(contextA.CurrentInstance);
            Assert.Collection(
                contextA.AppliedDecorators,
                item => Assert.IsType<DecoratorA<int>>(item),
                item => Assert.IsType<DecoratorB<int>>(item));
            Assert.Collection(
                contextA.AppliedDecoratorTypes,
                item => Assert.Equal(typeof(DecoratorA<int>), item),
                item => Assert.Equal(typeof(DecoratorB<int>), item));
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).SingleInstance();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));

            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();
            Assert.Same(instance, container.Resolve<IDecoratedService<int>>());

            using (var scope = container.BeginLifetimeScope())
            {
                Assert.Same(instance, scope.Resolve<IDecoratedService<int>>());
            }
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).InstancePerDependency();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));

            var container = builder.Build();

            var first = container.Resolve<IDecoratedService<int>>();
            var second = container.Resolve<IDecoratedService<int>>();
            Assert.NotSame(first, second);

            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<IDecoratedService<int>>();
                Assert.NotSame(first, third);
                Assert.NotSame(second, third);
            }
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));

            var container = builder.Build();

            var first = container.Resolve<IDecoratedService<int>>();
            var second = container.Resolve<IDecoratedService<int>>();
            Assert.Same(first, second);

            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<IDecoratedService<int>>();
                Assert.NotSame(first, third);

                var forth = scope.Resolve<IDecoratedService<int>>();
                Assert.Same(third, forth);
            }
        }

        [Fact]
        public void ParametersArePassedThroughDecoratorChain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorWithParameter<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var parameter = new NamedParameter("parameter", "ABC");
            var instance = container.Resolve<IDecoratedService<int>>(parameter);

            Assert.Equal("ABC", ((DecoratorWithParameter<int>)instance.Decorated).Parameter);
        }

        [Fact]
        public void ParametersCanBeConfiguredOnDecoratedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorWithParameters<>)).As(typeof(IDecoratedService<>)).WithParameter("parameter", "ABC");
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.Equal("ABC", ((ImplementorWithParameters<int>)instance.Decorated.Decorated).Parameter);
        }

        [Fact]
        public void CanResolveClosedGenericDecoratorOverOpenGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterDecorator(typeof(StringImplementor), typeof(IDecoratedService<string>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<string>>();

            Assert.IsType<StringImplementor>(instance);
            Assert.IsType<ImplementorA<string>>(instance.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedToServiceRegisteredInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)));
            var instance = scope.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(instance);
        }
    }
}
