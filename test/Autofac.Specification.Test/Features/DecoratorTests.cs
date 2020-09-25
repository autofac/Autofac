// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Decorators;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class DecoratorTests
    {
        private interface IDecoratedService : IService
        {
            IDecoratedService Decorated { get; }
        }

        private interface IDecoratorWithContext
        {
            IDecoratorContext Context { get; }
        }

        private interface IDecoratorWithParameter
        {
            string Parameter { get; }
        }

        private interface IService
        {
        }

        private interface ISomeOtherService
        {
        }

        [Fact]
        public void CanApplyDecoratorConditionallyAtRuntime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
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
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
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
        public void CanResolveDecoratorWithFunc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var factory = container.Resolve<Func<IDecoratedService>>();

            var decoratedService = factory();
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
        }

        [Fact]
        public void CanResolveDecoratorWithMeta()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().WithMetadata("A", 123);
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var meta = container.Resolve<Meta<IDecoratedService>>();

            var decoratedService = meta.Value;
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
            Assert.Equal(123, meta.Metadata["A"]);
        }

        [Fact]
        public void CanResolveDecoratorWithStronglyTypedMeta()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().WithMetadata("A", 123);
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var meta = container.Resolve<Meta<IDecoratedService, MyMetadata>>();

            var decoratedService = meta.Value;
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
            Assert.Equal(123, meta.Metadata.A);
        }

        [Fact]
        public void CanResolveDecoratorWithLazy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var lazy = container.Resolve<Lazy<IDecoratedService>>();

            var decoratedService = lazy.Value;
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
        }

        [Fact]
        public void CanResolveDecoratorWithLazyWithMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().WithMetadata("A", 123);
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var meta = container.Resolve<Lazy<IDecoratedService, MyMetadata>>();

            var decoratedService = meta.Value;
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
            Assert.Equal(123, meta.Metadata.A);
        }

        [Fact]
        public void CanResolveDecoratorWithOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var meta = container.Resolve<Owned<IDecoratedService>>();

            var decoratedService = meta.Value;
            Assert.IsType<DecoratorA>(decoratedService);
            Assert.IsType<ImplementorA>(decoratedService.Decorated);
        }

        [Fact]
        public void CanResolveMultipleDecoratedServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesWithMultipleDecorators()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().SingleInstance();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                });

            var services2 = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                services2,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                    Assert.Same(services[0], s); // single instance
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                    Assert.NotSame(services[1], s); // instance per dependency
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesWithMultipleDecoratorsSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().SingleInstance();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                });

            var services2 = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                services2,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                    Assert.Same(services[0], s); // single instance
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                    Assert.NotSame(services[1], s); // instance per dependency
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>().InstancePerLifetimeScope();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            IDecoratedService[] innerServices;

            using (var innerScope = container.BeginLifetimeScope())
            {
                innerServices = innerScope.Resolve<IEnumerable<IDecoratedService>>().ToArray();
                Assert.Collection(
                    innerServices,
                    s =>
                    {
                        Assert.IsType<DecoratorA>(s);
                        Assert.IsType<ImplementorA>(s.Decorated);
                    },
                    s =>
                    {
                        Assert.IsType<DecoratorA>(s);
                        Assert.IsType<ImplementorB>(s.Decorated);
                    });
            }

            var outerServices = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                outerServices,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                    Assert.NotSame(innerServices[0], s); // instance per dependency
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                    Assert.NotSame(innerServices[1], s); // instance per lifetime scope
                });

            var outerServices2 = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                outerServices2,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                    Assert.NotSame(outerServices[0], s); // instance per dependency
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                    Assert.Same(outerServices[1], s); // instance per lifetime scope
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesWithMultipleDecoratorsInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>().InstancePerLifetimeScope();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            IDecoratedService[] innerServices;

            using (var innerScope = container.BeginLifetimeScope())
            {
                innerServices = innerScope.Resolve<IEnumerable<IDecoratedService>>().ToArray();
                Assert.Collection(
                    innerServices,
                    s =>
                    {
                        Assert.IsType<DecoratorB>(s);
                        Assert.IsType<DecoratorA>(s.Decorated);
                        Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                    },
                    s =>
                    {
                        Assert.IsType<DecoratorB>(s);
                        Assert.IsType<DecoratorA>(s.Decorated);
                        Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                    });
            }

            var outerServices = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                outerServices,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                    Assert.NotSame(innerServices[0], s); // instance per dependency
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                    Assert.NotSame(innerServices[1], s); // instance per lifetime scope
                });

            var outerServices2 = container.Resolve<IEnumerable<IDecoratedService>>().ToArray();
            Assert.Collection(
                outerServices2,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                    Assert.NotSame(outerServices[0], s); // instance per dependency
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                    Assert.Same(outerServices[1], s); // instance per lifetime scope
                });
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesThenLatestService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                });

            var service = container.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorA>(service);
            Assert.IsType<ImplementorB>(service.Decorated);
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesWithMultipleDecoratorsThenLatestService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorA>(s.Decorated.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorB>(s);
                    Assert.IsType<DecoratorA>(s.Decorated);
                    Assert.IsType<ImplementorB>(s.Decorated.Decorated);
                });

            var service = container.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorB>(service);
            Assert.IsType<DecoratorA>(service.Decorated);
            Assert.IsType<ImplementorB>(service.Decorated.Decorated);
        }

        [Fact]
        public void CanResolveMultipleDecoratedServicesThenLatestServiceWithSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().SingleInstance();
            builder.RegisterType<ImplementorB>().As<IDecoratedService>().SingleInstance();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var services = container.Resolve<IEnumerable<IDecoratedService>>();

            Assert.Collection(
                services,
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorA>(s.Decorated);
                },
                s =>
                {
                    Assert.IsType<DecoratorA>(s);
                    Assert.IsType<ImplementorB>(s.Decorated);
                });

            var service = container.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorA>(service);
            Assert.IsType<ImplementorB>(service.Decorated);
        }

        [Fact]
        public void DecoratedRegistrationCanIncludeImplementationType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().AsSelf();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            Assert.IsType<ImplementorA>(container.Resolve<ImplementorA>());
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposableImplementor>().
                As<IDecoratedService>()
                .InstancePerDependency();
            builder.RegisterDecorator<DisposableDecorator, IDecoratedService>();
            var container = builder.Build();

            DisposableDecorator decorator;
            DisposableImplementor decorated;

            using (var scope = container.BeginLifetimeScope())
            {
                var instance = scope.Resolve<IDecoratedService>();
                decorator = (DisposableDecorator)instance;
                decorated = (DisposableImplementor)instance.Decorated;
            }

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposableImplementor>()
                .As<IDecoratedService>()
                .InstancePerLifetimeScope();
            builder.RegisterDecorator<DisposableDecorator, IDecoratedService>();
            var container = builder.Build();

            DisposableDecorator decorator;
            DisposableImplementor decorated;

            using (var scope = container.BeginLifetimeScope())
            {
                var instance = scope.Resolve<IDecoratedService>();
                decorator = (DisposableDecorator)instance;
                decorated = (DisposableImplementor)instance.Decorated;
            }

            var instance2 = container.Resolve<IDecoratedService>();
            Assert.NotSame(decorator, instance2);

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAndDecoratedBothDisposedWhenInstancePerMatchingLifetimeScope()
        {
            const string tag = "foo";

            var builder = new ContainerBuilder();

            builder.RegisterType<DisposableImplementor>()
                .As<IDecoratedService>()
                .InstancePerMatchingLifetimeScope(tag);
            builder.RegisterDecorator<DisposableDecorator, IDecoratedService>();
            var container = builder.Build();

            DisposableDecorator decorator;
            DisposableImplementor decorated;

            using (var scope = container.BeginLifetimeScope(tag))
            {
                var instance = scope.Resolve<IDecoratedService>();
                decorator = (DisposableDecorator)instance;
                decorated = (DisposableImplementor)instance.Decorated;

                DisposableDecorator decorator2;
                DisposableImplementor decorated2;

                using (var scope2 = scope.BeginLifetimeScope())
                {
                    var instance2 = scope2.Resolve<IDecoratedService>();
                    decorator2 = (DisposableDecorator)instance2;
                    decorated2 = (DisposableImplementor)instance2.Decorated;
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

            builder.RegisterType<DisposableImplementor>()
                .As<IDecoratedService>()
                .SingleInstance();
            builder.RegisterDecorator<DisposableDecorator, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();
            container.Dispose();

            var decorator = (DisposableDecorator)instance;
            var decorated = (DisposableImplementor)instance.Decorated;

            Assert.Equal(1, decorator.DisposeCallCount);
            Assert.Equal(1, decorated.DisposeCallCount);
        }

        [Fact]
        public void DecoratorAppliedOnlyOnceToComponentWithExternalRegistrySource()
        {
            // #965: A nested lifetime scope that has a registration lambda
            // causes the decorator to be applied twice - once for the container
            // level, and once for the scope level.
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => { });
            var service = scope.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(service);
            Assert.IsType<ImplementorA>(service.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedToServiceRegisteredInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterType<ImplementorA>().As<IDecoratedService>());
            var instance = scope.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwice()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var service = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(service);
            Assert.IsType<DecoratorA>(service.Decorated);
            Assert.IsType<ImplementorA>(service.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwiceInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterDecorator<DecoratorA, IDecoratedService>());
            var scopeInstance = scope.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(scopeInstance);
            Assert.IsType<DecoratorA>(scopeInstance.Decorated);
            Assert.IsType<ImplementorA>(scopeInstance.Decorated.Decorated);

            var rootInstance = container.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorA>(rootInstance);
            Assert.IsType<ImplementorA>(rootInstance.Decorated);
        }

        [Fact]
        public void DecoratorCanBeAppliedTwiceIntentionallyWithExternalRegistrySource()
        {
            // #965: A nested lifetime scope that has a registration lambda
            // causes the decorator to be applied twice - once for the container
            // level, and once for the scope level.
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => { });
            var service = scope.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(service);
            Assert.IsType<DecoratorA>(service.Decorated);
            Assert.IsType<ImplementorA>(service.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorCanBeRegisteredInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(b => b.RegisterDecorator<DecoratorA, IDecoratedService>());

            var scopedInstance = scope.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorA>(scopedInstance);
            Assert.IsType<ImplementorA>(scopedInstance.Decorated);

            var rootInstance = container.Resolve<IDecoratedService>();
            Assert.IsType<ImplementorA>(rootInstance);
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().InstancePerDependency();
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
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().InstancePerLifetimeScope();
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
        public void DecoratorInheritsDecoratedLifetimeWhenInstancePerMatchingLifetimeScope()
        {
            const string tag = "foo";

            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>()
                .As<IDecoratedService>()
                .InstancePerMatchingLifetimeScope(tag);
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope(tag))
            {
                var first = scope.Resolve<IDecoratedService>();
                var second = scope.Resolve<IDecoratedService>();
                Assert.Same(first, second);

                using (var scope2 = scope.BeginLifetimeScope())
                {
                    var third = scope2.Resolve<IDecoratedService>();
                    Assert.Same(second, third);
                }
            }
        }

        [Fact]
        public void DecoratorInheritsDecoratedLifetimeWhenSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().SingleInstance();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();

            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();
            Assert.Same(instance, container.Resolve<IDecoratedService>());

            using (var scope = container.BeginLifetimeScope())
            {
                Assert.Same(instance, scope.Resolve<IDecoratedService>());
            }
        }

        [Fact(Skip = "Issue #967")]
        public void DecoratorParameterSupportsFunc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorWithFunc, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorWithFunc>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact(Skip = "Issue #967")]
        public void DecoratorParameterSupportsLazy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorWithLazy, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorWithLazy>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void DecoratorRegisteredAsLambdaCanAcceptAdditionalParameters()
        {
            const string parameterName = "parameter";
            const string parameterValue = "ABC";

            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<IDecoratedService>((c, p, i) =>
            {
                var stringParameter = (string)p
                    .OfType<NamedParameter>()
                    .FirstOrDefault(np => np.Name == parameterName)?.Value;

                return new DecoratorWithParameter(i, stringParameter);
            });
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var parameter = new NamedParameter(parameterName, parameterValue);
            var instance = container.Resolve<IDecoratedService>(parameter);

            Assert.Equal(parameterValue, ((DecoratorWithParameter)instance.Decorated).Parameter);
        }

        [Fact]
        public void DecoratorRegisteredAsLambdaCanBeResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<IDecoratedService>((c, p, i) => new DecoratorA(i));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void DecoratorRegisteredOnLambdaWithCast()
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => (IDecoratedService)new ImplementorA()).As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();
            var instance = container.Resolve<IDecoratedService>();
            Assert.IsType<DecoratorA>(instance);
        }

        [Fact]
        public void DecoratorRegistrationsGetAppliedInOrderAdded()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);

            builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            container = builder.Build();

            instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<DecoratorB>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void DecoratorsApplyToKeyedServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().Keyed<IDecoratedService>("service");
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.ResolveKeyed<IDecoratedService>("service");

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void DecoratorsApplyToNamedAndDefaultServices()
        {
            // Issue #529, #880: Old decorator syntax failed if a component
            // being decorated was registered with both As<T>() and Named<T>().
            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().As<IDecoratedService>().Named<IDecoratedService>("service");
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Theory]
        [InlineData(typeof(IDecoratedService), typeof(ISomeOtherService))]
        [InlineData(typeof(ISomeOtherService), typeof(IDecoratedService))]
        public void DecoratorShouldBeAppliedRegardlessOfServiceOrder(Type firstService, Type secondService)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorWithSomeOtherService>().As(firstService, secondService);
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorWithSomeOtherService>(instance.Decorated);
        }

        [Fact]
        public void ParametersArePassedThroughDecoratorChain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
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
            builder.RegisterType<ImplementorWithParameters>().As<IDecoratedService>().WithParameter("parameter", "ABC");
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.Equal("ABC", ((ImplementorWithParameters)instance.Decorated.Decorated).Parameter);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenMultipleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            builder.RegisterDecorator<DecoratorB, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenNoDecoratorsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<ImplementorA>(instance);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenRegisteredWithoutGenericConstraint()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator(typeof(DecoratorA), typeof(IDecoratedService));
            builder.RegisterDecorator(typeof(DecoratorB), typeof(IDecoratedService));
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorB>(instance);
            Assert.IsType<DecoratorA>(instance.Decorated);
            Assert.IsType<ImplementorA>(instance.Decorated.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenSingleDecoratorRegistered()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenTargetHasOnActivatingHandlerOnType()
        {
            var activatingInstances = new List<object>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().As<IDecoratedService>()
                .OnActivating(args => activatingInstances.Add(args.Instance));
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);

            Assert.Single(activatingInstances);
            Assert.IsType<ImplementorA>(activatingInstances[0]);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenTargetHasOnActivatingHandlerOnInterface()
        {
            var activatingInstances = new List<object>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().AsSelf();
            builder.Register<IDecoratedService>(c => c.Resolve<ImplementorA>())
                .OnActivating(args => activatingInstances.Add(args.Instance));
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);

            Assert.Single(activatingInstances);
            Assert.IsType<ImplementorA>(activatingInstances[0]);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenTargetHasOnActivatedHandlerOnType()
        {
            var activatedInstances = new List<object>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().As<IDecoratedService>()
                .OnActivated(args => activatedInstances.Add(args.Instance));
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);

            Assert.Single(activatedInstances);
            Assert.IsType<ImplementorA>(activatedInstances[0]);
        }

        [Fact]
        public void ResolvesDecoratedServiceWhenTargetHasOnActivatedHandlerOnInterface()
        {
            var activatedInstances = new List<object>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().AsSelf();
            builder.Register<IDecoratedService>(c => c.Resolve<ImplementorA>())
                .OnActivated(args => activatedInstances.Add(args.Instance));
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var instance = container.Resolve<IDecoratedService>();

            Assert.IsType<DecoratorA>(instance);
            Assert.IsType<ImplementorA>(instance.Decorated);

            Assert.Single(activatedInstances);
            Assert.IsType<ImplementorA>(activatedInstances[0]);
        }

        [Fact]
        public void StartableTypesCanBeDecorated()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StartableImplementation>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterDecorator<StartableDecorator, IStartable>();
            var container = builder.Build();

            var startable = container.Resolve<IStartable>();
            var decorated = Assert.IsType<StartableDecorator>(startable);
            var implementation = Assert.IsType<StartableImplementation>(decorated.Decorated);
            Assert.True(implementation.Started);
        }

        private class MyMetadata
        {
            public int A { get; set; }
        }

        private abstract class Decorator : IDecoratedService
        {
            protected Decorator(IDecoratedService decorated)
            {
                Decorated = decorated;
            }

            public IDecoratedService Decorated { get; }
        }

        private class DecoratorA : Decorator
        {
            public DecoratorA(IDecoratedService decorated)
                : base(decorated)
            {
            }
        }

        private class DecoratorB : Decorator
        {
            public DecoratorB(IDecoratedService decorated)
                : base(decorated)
            {
            }
        }

        private class DecoratorWithContextA : Decorator, IDecoratorWithContext
        {
            public DecoratorWithContextA(IDecoratedService decorated, IDecoratorContext context)
                : base(decorated)
            {
                Context = context;
            }

            public IDecoratorContext Context { get; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DecoratorWithContextB : Decorator, IDecoratorWithContext
        {
            public DecoratorWithContextB(IDecoratedService decorated, IDecoratorContext context)
                : base(decorated)
            {
                Context = context;
            }

            public IDecoratorContext Context { get; }
        }

        private class DecoratorWithFunc : IDecoratedService
        {
            public DecoratorWithFunc(Func<IDecoratedService> decorated)
            {
                Decorated = decorated();
            }

            public IDecoratedService Decorated { get; }
        }

        private class DecoratorWithLazy : IDecoratedService
        {
            public DecoratorWithLazy(Lazy<IDecoratedService> decorated)
            {
                Decorated = decorated.Value;
            }

            public IDecoratedService Decorated { get; }
        }

        private class DecoratorWithParameter : Decorator, IDecoratorWithParameter
        {
            public DecoratorWithParameter(IDecoratedService decorated, string parameter)
                : base(decorated)
            {
                Parameter = parameter;
            }

            public string Parameter { get; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DisposableDecorator : Decorator, IDisposable
        {
            public DisposableDecorator(IDecoratedService decorated)
                : base(decorated)
            {
            }

            public int DisposeCallCount { get; private set; }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DisposableImplementor : IDecoratedService, IDisposable
        {
            public IDecoratedService Decorated => this;

            public int DisposeCallCount { get; private set; }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }

        private class ImplementorA : IDecoratedService
        {
            public IDecoratedService Decorated => this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ImplementorB : IDecoratedService
        {
            public IDecoratedService Decorated => this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ImplementorWithParameters : IDecoratedService
        {
            public ImplementorWithParameters(string parameter)
            {
                Parameter = parameter;
            }

            public IDecoratedService Decorated => this;

            public string Parameter { get; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ImplementorWithSomeOtherService : IDecoratedService, ISomeOtherService
        {
            public IDecoratedService Decorated => this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class StartableImplementation : IStartable
        {
            public IStartable Decorated => this;

            public bool Started { get; private set; }

            public void Start()
            {
                Started = true;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class StartableDecorator : IStartable
        {
            public IStartable Decorated { get; }

            public StartableDecorator(IStartable startable)
            {
                Decorated = startable;
            }

            public void Start()
            {
                Decorated.Start();
            }
        }
    }
}
