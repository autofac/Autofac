using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;

namespace Autofac.Test.Features.Variance
{
    interface IHandler<in TCommand>
    {
        void Handle(TCommand command);
    }

    interface IBaseCommand { }

    interface ICommand : IBaseCommand { }

    interface IDerivedCommand : ICommand { }

    class InterfaceHandler : IHandler<ICommand>
    {
        public void Handle(ICommand command) { }
    }

    class CommandA { }

    class CommandB : CommandA, ICommand { }

    class CommandC : CommandB { }

    class CommandD : CommandC { }

    class BHandler : IHandler<CommandB>
    {
        public void Handle(CommandB command) { }
    }

    class ObjectHandler : IHandler<object>
    {
        public void Handle(object command) { }
    }

    class UnrelatedCommand { }

    interface IConstrainedHandler<in TCommand>
        where TCommand : new() { }

    class BaseWithArg
    {
        public BaseWithArg(int arg) { }
    }

    class DerivedWithoutArg : BaseWithArg
    {
        public DerivedWithoutArg() : base(0) { }
    }

    static class HandlerTestExtensions
    {
        public static void AssertSingleHandlerCanHandle<TCommand>(this IContainer container)
        {
            var handlers = container.ComponentRegistry.RegistrationsFor(new TypedService(typeof(IHandler<TCommand>)));
            Assert.Equal(1, handlers.Count());
            container.Resolve<IHandler<TCommand>>();
        }
    }

    public class ContravariantRegistrationSourceTests
    {
        public class WhenAHandlerForAConcreteTypeIsRegistered
        {
            IContainer _container;

            public WhenAHandlerForAConcreteTypeIsRegistered()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<BHandler>().As<IHandler<CommandB>>();
                _container = builder.Build();
            }

            [Fact]
            public void TheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandB>();
            }

            [Fact]
            public void DirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandC>();
            }

            [Fact]
            public void IndirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandD>();
            }

            [Fact]
            public void BaseClassesOfTheCommandCannotBeHandled()
            {
                Assert.False(_container.IsRegistered<IHandler<CommandA>>());
            }

            [Fact]
            public void UnrelatedCommandsCannotBeHandled()
            {
                Assert.False(_container.IsRegistered<IHandler<UnrelatedCommand>>());
            }
        }
        public class WhenAHandlerForAnInterfaceTypeIsRegistered
        {
            IContainer _container;

            public WhenAHandlerForAnInterfaceTypeIsRegistered()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<InterfaceHandler>().As<IHandler<ICommand>>();
                _container = builder.Build();
            }

            [Fact]
            public void TheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<ICommand>();
            }

            [Fact]
            public void ImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandB>();
            }

            [Fact]
            public void IndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandC>();
            }

            [Fact]
            public void SecondOrderIndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandD>();
            }

            [Fact]
            public void DerivationsOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<IDerivedCommand>();
            }

            [Fact]
            public void UnrelatedCommandsCannotBeHandled()
            {
                Assert.False(_container.IsRegistered<IHandler<UnrelatedCommand>>());
            }

            [Fact]
            public void BaseInterfacesOfTheCommandCannotBeHandled()
            {
                Assert.False(_container.IsRegistered<IHandler<IBaseCommand>>());
            }
        }
        public class WhenAHandlerForObjectIsRegistered
        {
            IContainer _container;

            public WhenAHandlerForObjectIsRegistered()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<ObjectHandler>().As<IHandler<object>>();
                _container = builder.Build();
            }

            [Fact]
            public void ObjectCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<object>();
            }

            [Fact]
            public void AnyConcreteTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandA>();
            }

            [Fact]
            public void AnyInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<ICommand>();
            }
        }
        public class WhenBaseTypesDoNotSatisfyConstraints
        {
            [Fact]
            public void TheSourceDoesNotAttemptGenericTypeConstruction()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                var container = builder.Build();
                Assert.False(container.IsRegistered<IConstrainedHandler<DerivedWithoutArg>>());
            }
        }
    }
}
