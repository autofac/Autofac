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

        // ReSharper disable once UnusedTypeParameter
        public interface ISomeOtherService<T>
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

        public class ImplementorWithSomeOtherService<T> : IDecoratedService<T>, ISomeOtherService<T>
        {
            public IDecoratedService<T> Decorated => this;
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

        public class DisposableImplementor<T> : IDecoratedService<T>, IDisposable
        {
            public int DisposeCallCount { get; private set; }

            public IDecoratedService<T> Decorated => this;

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }

        public class DisposableDecorator<T> : Decorator<T>, IDisposable
        {
            public int DisposeCallCount { get; private set; }

            public DisposableDecorator(IDecoratedService<T> decorated)
                : base(decorated)
            {
            }

            public void Dispose()
            {
                DisposeCallCount++;
            }
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
            Assert.Equal(typeof(ImplementorA<int>), registration.Target.Activator.LimitType);
        }

        [Fact]
        public void DecoratedRegistrationCanIncludeImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).AsSelf();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            Assert.IsType<ImplementorA<int>>(container.Resolve<ImplementorA<int>>());
        }

        [Fact]
        public void DecoratedInstancePerDependencyRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).As(typeof(IService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IService<>));
            var container = builder.Build();

            var serviceRegistration = container.RegistrationFor<IService<int>>();
            var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService<int>>();

            Assert.NotNull(serviceRegistration);
            Assert.NotNull(decoratedServiceRegistration);
            Assert.Same(serviceRegistration, decoratedServiceRegistration);

            var serviceInstance = container.Resolve<IService<int>>();
            Assert.IsType<DecoratorA<int>>(serviceInstance);

            var decoratedServiceInstance = container.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(decoratedServiceInstance);

            Assert.NotSame(serviceInstance, decoratedServiceInstance);
        }

        [Fact(Skip = "Issue #963")]
        public void DecoratedInstancePerLifetimeScopeRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();

            // #963: The InstancePerLifetimeScope here is important - a single component may expose multiple services.
            // If that component is decorated, the decorator ALSO needs to expose all of those services.
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).As(typeof(IService<>)).InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var serviceRegistration = container.RegistrationFor<IService<int>>();
            var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService<int>>();

            Assert.NotNull(serviceRegistration);
            Assert.NotNull(decoratedServiceRegistration);
            Assert.Same(serviceRegistration, decoratedServiceRegistration);

            var serviceInstance = container.Resolve<IService<int>>();
            Assert.IsType<DecoratorA<int>>(serviceInstance);

            var decoratedServiceInstance = container.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(decoratedServiceInstance);

            Assert.Same(serviceInstance, decoratedServiceInstance);
        }

        [Fact(Skip = "Issue #963")]
        public void DecoratedSingleInstanceRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();

            // #963: The SingleInstance here is important - a single component may expose multiple services.
            // If that component is decorated, the decorator ALSO needs to expose all of those services.
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>)).As(typeof(IService<>)).SingleInstance();
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var serviceRegistration = container.RegistrationFor<IService<int>>();
            var decoratedServiceRegistration = container.RegistrationFor<IDecoratedService<int>>();

            Assert.NotNull(serviceRegistration);
            Assert.NotNull(decoratedServiceRegistration);
            Assert.Same(serviceRegistration, decoratedServiceRegistration);

            var serviceInstance = container.Resolve<IService<int>>();
            Assert.IsType<DecoratorA<int>>(serviceInstance);

            var decoratedServiceInstance = container.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(decoratedServiceInstance);

            Assert.Same(serviceInstance, decoratedServiceInstance);
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

            var decoratedService = factory();
            Assert.IsType<DecoratorA<int>>(decoratedService);
            Assert.IsType<ImplementorA<int>>(decoratedService.Decorated);
        }

        [Fact]
        public void CanResolveDecoratorWithLazy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var lazy = container.Resolve<Lazy<IDecoratedService<int>>>();

            var decoratedService = lazy.Value;
            Assert.IsType<DecoratorA<int>>(decoratedService);
            Assert.IsType<ImplementorA<int>>(decoratedService.Decorated);
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
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerMatchingLifetimeScope()
        {
            const string tag = "foo";

            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>))
                .As(typeof(IDecoratedService<>))
                .InstancePerMatchingLifetimeScope(tag);
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope(tag))
            {
                var first = scope.Resolve<IDecoratedService<int>>();
                var second = scope.Resolve<IDecoratedService<int>>();
                Assert.Same(first, second);

                using (var scope2 = scope.BeginLifetimeScope())
                {
                    var third = scope2.Resolve<IDecoratedService<int>>();
                    Assert.Same(second, third);
                }
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
        public void DecoratorAppliedOnlyOnceToComponentWithExternalRegistrySource()
        {
            // #965: A nested lifetime scope that has a registration lambda
            // causes the decorator to be applied twice - once for the container
            // level, and once for the scope level.
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => { });
            var service = scope.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(service);
            Assert.IsType<ImplementorA<int>>(service.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwiceIntentionallyWithExternalRegistrySource()
        {
            // #965: A nested lifetime scope that has a registration lambda
            // causes the decorator to be applied twice - once for the container
            // level, and once for the scope level.
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => { });
            var service = scope.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(service);
            Assert.IsType<DecoratorA<int>>(service.Decorated);
            Assert.IsType<ImplementorA<int>>(service.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwice()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var service = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(service);
            Assert.IsType<DecoratorA<int>>(service.Decorated);
            Assert.IsType<ImplementorA<int>>(service.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwiceInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>)));
            var scopeInstance = scope.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(scopeInstance);
            Assert.IsType<DecoratorA<int>>(scopeInstance.Decorated);
            Assert.IsType<ImplementorA<int>>(scopeInstance.Decorated.Decorated);

            var rootInstance = container.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(rootInstance);
            Assert.IsType<ImplementorA<int>>(rootInstance.Decorated);
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
            Assert.IsType<ImplementorA<int>>(instance.Decorated);
        }

        [Fact]
        public void DecoratorCanBeRegisteredInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(ImplementorA<>)).As(typeof(IDecoratedService<>));
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>)));

            var scopedInstance = scope.Resolve<IDecoratedService<int>>();
            Assert.IsType<DecoratorA<int>>(scopedInstance);
            Assert.IsType<ImplementorA<int>>(scopedInstance.Decorated);

            var rootInstance = container.Resolve<IDecoratedService<int>>();
            Assert.IsType<ImplementorA<int>>(rootInstance);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerDependency()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(DisposableImplementor<>))
                .As(typeof(IDecoratedService<>))
                .InstancePerDependency();
            builder.RegisterGenericDecorator(typeof(DisposableDecorator<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            DisposableDecorator<int> decorator;
            DisposableImplementor<int> decorated;

            using (var scope = container.BeginLifetimeScope())
            {
                var instance = scope.Resolve<IDecoratedService<int>>();
                decorator = (DisposableDecorator<int>)instance;
                decorated = (DisposableImplementor<int>)instance.Decorated;
            }

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(DisposableImplementor<>))
                .As(typeof(IDecoratedService<>))
                .InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(typeof(DisposableDecorator<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            DisposableDecorator<int> decorator;
            DisposableImplementor<int> decorated;

            using (var scope = container.BeginLifetimeScope())
            {
                var instance = scope.Resolve<IDecoratedService<int>>();
                decorator = (DisposableDecorator<int>)instance;
                decorated = (DisposableImplementor<int>)instance.Decorated;
            }

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerMatchingLifetimeScope()
        {
            const string tag = "foo";

            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(DisposableImplementor<>))
                .As(typeof(IDecoratedService<>))
                .InstancePerMatchingLifetimeScope(tag);
            builder.RegisterGenericDecorator(typeof(DisposableDecorator<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            DisposableDecorator<int> decorator;
            DisposableImplementor<int> decorated;

            using (var scope = container.BeginLifetimeScope(tag))
            {
                var instance = scope.Resolve<IDecoratedService<int>>();
                decorator = (DisposableDecorator<int>)instance;
                decorated = (DisposableImplementor<int>)instance.Decorated;

                DisposableDecorator<int> decorator2;
                DisposableImplementor<int> decorated2;

                using (var scope2 = scope.BeginLifetimeScope())
                {
                    var instance2 = scope2.Resolve<IDecoratedService<int>>();
                    decorator2 = (DisposableDecorator<int>)instance2;
                    decorated2 = (DisposableImplementor<int>)instance2.Decorated;
                }

                Assert.Equal(0, decorator2.DisposeCallCount);
                Assert.Equal(0, decorated2.DisposeCallCount);
            }

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenSingleInstance()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(DisposableImplementor<>))
                .As(typeof(IDecoratedService<>))
                .SingleInstance();
            builder.RegisterGenericDecorator(typeof(DisposableDecorator<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();
            container.Dispose();

            var decorator = (DisposableDecorator<int>)instance;
            var decorated = (DisposableImplementor<int>)instance.Decorated;

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Theory]
        [InlineData(typeof(IDecoratedService<>), typeof(ISomeOtherService<>))]
        [InlineData(typeof(ISomeOtherService<>), typeof(IDecoratedService<>))]
        public void DecoratorShouldBeAppliedRegardlessOfServiceOrder(Type firstService, Type secondService)
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ImplementorWithSomeOtherService<>)).As(firstService, secondService);
            builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IDecoratedService<>));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService<int>>();

            Assert.IsType<DecoratorA<int>>(instance);
            Assert.IsType<ImplementorWithSomeOtherService<int>>(instance.Decorated);
        }

        [Fact]
        public void CanApplyDecoratorOnTypeThatImplementsTwoInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(TransactionalCommandHandlerDecorator<>).Assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .Where(type => !typeof(TransactionalCommandHandlerDecorator<>).IsAssignableFrom(type))
                .InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(typeof(TransactionalCommandHandlerDecorator<>), typeof(ICommandHandler<>));
            var container = builder.Build();

            var instance = container.Resolve<ICommandHandler<CreateLocation>>();

            Assert.IsType<TransactionalCommandHandlerDecorator<CreateLocation>>(instance);
        }

        public interface ICommandHandler<T>
        {
            void Handle(T command);
        }

        public class TransactionalCommandHandlerDecorator<T> : ICommandHandler<T>
        {
            public ICommandHandler<T> Handler { get; }

            public TransactionalCommandHandlerDecorator(ICommandHandler<T> handler)
            {
                Handler = handler;
            }

            public void Handle(T command)
            {
            }
        }

        public class ModifyLocation
        {
        }

        public class CreateLocation
        {
        }

        public class Handler : ICommandHandler<CreateLocation>, ICommandHandler<ModifyLocation>
        {
            public void Handle(CreateLocation command)
            {

            }

            public void Handle(ModifyLocation command)
            {
            }
        }
    }
}
