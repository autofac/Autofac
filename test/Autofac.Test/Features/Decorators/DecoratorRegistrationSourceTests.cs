using System;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Decorators;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class DecoratorRegistrationSourceTests
    {
        public interface IService
        {
        }

        public interface IDecoratedService : IService
        {
            IDecoratedService Decorated { get; }
        }

        public class ImplementorA : IDecoratedService
        {
            public IDecoratedService Decorated => this;
        }

        public class ImplementorB : IDecoratedService
        {
            public IDecoratedService Decorated => this;
        }

        public class ImplementorWithParameters : IDecoratedService
        {
            public IDecoratedService Decorated => this;

            public string Parameter { get; }

            public ImplementorWithParameters(string parameter)
            {
                Parameter = parameter;
            }
        }

        public abstract class Decorator : IDecoratedService
        {
            protected Decorator(IDecoratedService decorated)
            {
                Decorated = decorated;
            }

            public IDecoratedService Decorated { get; }
        }

        public class DecoratorA : Decorator
        {
            public DecoratorA(IDecoratedService decorated)
                : base(decorated)
            {
            }
        }

        public class DecoratorB : Decorator
        {
            public DecoratorB(IDecoratedService decorated)
                : base(decorated)
            {
            }
        }

        public interface IDecoratorWithParameter
        {
            string Parameter { get; }
        }

        public class DecoratorWithParameter : Decorator, IDecoratorWithParameter
        {
            public DecoratorWithParameter(IDecoratedService decorated, string parameter)
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

        public class DecoratorWithContextA : Decorator, IDecoratorWithContext
        {
            public DecoratorWithContextA(IDecoratedService decorated, IDecoratorContext context)
                : base(decorated)
            {
                Context = context;
            }

            public IDecoratorContext Context { get; }
        }

        public class DecoratorWithContextB : Decorator, IDecoratorWithContext
        {
            public DecoratorWithContextB(IDecoratedService decorated, IDecoratorContext context)
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
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var registration = container.RegistrationFor<IDecoratedService>();
            Assert.NotNull(registration);

            var decoratedService = new TypedService(typeof(IDecoratedService));
            Assert.Contains(registration.Services.OfType<TypedService>(), s => s == decoratedService);
        }

        [Fact]
        public void RegistrationTargetsTheImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var registration = container.RegistrationFor<IDecoratedService>();

            Assert.NotNull(registration);
            Assert.Equal(registration.Target.Activator.LimitType, typeof(ImplementorA));
        }

        [Fact]
        public void DecoratedRegistrationCannotIncludeImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>().As<ImplementorA>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void DecoratedRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>().As<IService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var serviceRegistration = container.RegistrationFor<IService>();
            var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService>();

            Assert.NotNull(serviceRegistration);
            Assert.NotNull(decoratedServiceRegistration);
            Assert.Same(serviceRegistration, decoratedServiceRegistration);

            Assert.IsType<DecoratorA>(container.Resolve<IService>());
            Assert.IsType<DecoratorA>(container.Resolve<IDecoratedService>());
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenNoDecoratorsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<ImplementorA>(instance);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenSingleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenMultipleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenRegisteredWithoutGenericConstraint()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated(typeof(ImplementorA), typeof(IDecoratedService));
            builder.RegisterDecorator(typeof(DecoratorA), typeof(IDecoratedService));
            builder.RegisterDecorator(typeof(DecoratorB), typeof(IDecoratedService));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorRegistrationsGetAppliedInOrderAdded()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);

            builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            container = builder.Build();

            instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<DecoratorB>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void CanApplyDecoratorConditionallyAtRuntime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>(context => context.AppliedDecorators.Any());
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void CanInjectDecoratorContextAsSnapshot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            builder.RegisterDecorator<DecoratorWithContextA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorWithContextB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            var contextB = ((IDecoratorWithContext)instance).Context;
            Assert.Equal(typeof(IDecoratedService), contextB.ServiceType);
            Assert.Equal(typeof(ImplementorA), contextB.ImplementationType);
            Assert.IsType<DecoratorWithContextA>(contextB.CurrentInstance);
            Assert.Collection(
                contextB.AppliedDecorators,
                item => Assert.IsType<DecoratorA>(item),
                item => Assert.IsType<DecoratorB>(item),
                item => Assert.IsType<DecoratorWithContextA>(item));
            Assert.Collection(
                contextB.AppliedDecoratorTypes,
                item => Assert.Equal(typeof(DecoratorA), item),
                item => Assert.Equal(typeof(DecoratorB), item),
                item => Assert.Equal(typeof(DecoratorWithContextA), item));

            var contextA = ((IDecoratorWithContext)instance.Decorated).Context;
            Assert.Equal(typeof(IDecoratedService), contextA.ServiceType);
            Assert.Equal(typeof(ImplementorA), contextA.ImplementationType);
            Assert.IsType<DecoratorB>(contextA.CurrentInstance);
            Assert.Collection(
                contextA.AppliedDecorators,
                item => Assert.IsType<DecoratorA>(item),
                item => Assert.IsType<DecoratorB>(item));
            Assert.Collection(
                contextA.AppliedDecoratorTypes,
                item => Assert.Equal(typeof(DecoratorA), item),
                item => Assert.Equal(typeof(DecoratorB), item));
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>().SingleInstance();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();
            Assert.Same(instance, container.Resolve<IDecoratedService>());

            using (var scope = container.BeginLifetimeScope())
            {
                Assert.Same(instance, scope.Resolve<IDecoratedService>());
            }
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>().InstancePerDependency();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            var container = builder.Build();

            var first = container.Resolve<IDecoratedService>();
            var second = container.Resolve<IDecoratedService>();
            Assert.NotSame(first, second);

            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<IDecoratedService>();
                Assert.NotSame(first, third);
                Assert.NotSame(second, third);
            }
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>().InstancePerLifetimeScope();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            var container = builder.Build();

            var first = container.Resolve<IDecoratedService>();
            var second = container.Resolve<IDecoratedService>();
            Assert.Same(first, second);

            using (var scope = container.BeginLifetimeScope())
            {
                var third = scope.Resolve<IDecoratedService>();
                Assert.NotSame(first, third);

                var forth = scope.Resolve<IDecoratedService>();
                Assert.Same(third, forth);
            }
        }

        [Fact]
        public void ParametersArePassedThroughDecoratorChain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorWithParameter, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var parameter = new NamedParameter("parameter", "ABC");
            var instance = container.Resolve<IDecoratedService>(parameter);

            Assert.Equal("ABC", ((DecoratorWithParameter)instance.Decorated).Parameter);
        }

        [Fact]
        public void ParametersCanBeConfiguredOnDecoratedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorated<ImplementorWithParameters, IDecoratedService>().WithParameter("parameter", "ABC");
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.Equal("ABC", ((ImplementorWithParameters)instance.Decorated.Decorated).Parameter);
        }
    }
}
