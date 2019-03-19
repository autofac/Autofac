using System;
using System.Linq;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class DecoratorTests
    {
        private interface IDecoratedService : IService
        {
            IDecoratedService Decorated { get; }
        }

        private interface IService
        {
        }

        [Fact]
        public void DecoratedRegistrationCanIncludeOtherServices()
        {
            var builder = new ContainerBuilder();

            // #963: The SingleInstance here is important - a single component may expose multiple services.
            // If that component is decorated, the decorator ALSO needs to expose all of those services.
            builder.RegisterType<ImplementorA>().As<IDecoratedService>().As<IService>().SingleInstance();
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
        public void RegistrationIncludesTheServiceType()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
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
            builder.RegisterType<ImplementorA>().As<IDecoratedService>();
            builder.RegisterDecorator<DecoratorA, IDecoratedService>();
            var container = builder.Build();

            var registration = container.RegistrationFor<IDecoratedService>();

            Assert.NotNull(registration);
            Assert.Equal(typeof(ImplementorA), registration.Target.Activator.LimitType);
        }

        private abstract class Decorator : IDecoratedService
        {
            protected Decorator(IDecoratedService decorated)
            {
                this.Decorated = decorated;
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

        private class ImplementorA : IDecoratedService
        {
            public IDecoratedService Decorated => this;
        }
    }
}
