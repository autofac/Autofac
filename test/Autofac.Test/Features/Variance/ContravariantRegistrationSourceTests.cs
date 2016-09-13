using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;
using Xunit;

namespace Autofac.Test.Features.Variance
{
    public class ContravariantRegistrationSourceTests
    {
        internal interface IHandler<in TCommand>
        {
            void Handle(TCommand command);
        }

        internal interface IBaseCommand
        {
        }

        internal interface ICommand : IBaseCommand
        {
        }

        internal interface IDerivedCommand : ICommand
        {
        }

        internal class InterfaceHandler : IHandler<ICommand>
        {
            public void Handle(ICommand command)
            {
            }
        }

        internal class CommandA
        {
        }

        internal class CommandB : CommandA, ICommand
        {
        }

        internal class CommandC : CommandB
        {
        }

        internal class CommandD : CommandC
        {
        }

        internal class BHandler : IHandler<CommandB>
        {
            public void Handle(CommandB command)
        {
        }
        }

        internal class ObjectHandler : IHandler<object>
        {
            public void Handle(object command)
            {
            }
        }

        internal class UnrelatedCommand
        {
        }

        internal interface IConstrainedHandler<in TCommand>
            where TCommand : new()
        {
        }

        internal class BaseWithArg
        {
            public BaseWithArg(int arg)
            {
            }
        }

        internal class DerivedWithoutArg : BaseWithArg
        {
            public DerivedWithoutArg()
                : base(0)
            {
            }
        }

        internal enum AEnum
        {
            First,
            Second
        }

        internal static class AssertExtensions
        {
            public static void AssertSingleHandlerCanHandle<TCommand>(IContainer container)
            {
                var handlers = container.ComponentRegistry.RegistrationsFor(new TypedService(typeof(IHandler<TCommand>)));
                Assert.Equal(1, handlers.Count());
                container.Resolve<IHandler<TCommand>>();
            }
        }

        public class WhenAHandlerForAConcreteTypeIsRegistered
        {
            private IContainer _container;

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
                AssertExtensions.AssertSingleHandlerCanHandle<CommandB>(_container);
            }

            [Fact]
            public void DirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandC>(_container);
            }

            [Fact]
            public void IndirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandD>(_container);
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
            private IContainer _container;

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
                AssertExtensions.AssertSingleHandlerCanHandle<ICommand>(_container);
            }

            [Fact]
            public void ImplementersOfTheInterfaceTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandB>(_container);
            }

            [Fact]
            public void IndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandC>(_container);
            }

            [Fact]
            public void SecondOrderIndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandD>(_container);
            }

            [Fact]
            public void DerivationsOfTheInterfaceTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<IDerivedCommand>(_container);
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
            private IContainer _container;

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
                AssertExtensions.AssertSingleHandlerCanHandle<object>(_container);
            }

            [Fact]
            public void AnyConcreteTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<CommandA>(_container);
            }

            [Fact]
            public void AnyInterfaceTypeCanBeHandled()
            {
                AssertExtensions.AssertSingleHandlerCanHandle<ICommand>(_container);
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

        public class WhenAValueTypeIsRequested
        {
            [Fact]
            public void TheSourceDoesNotApply()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<ObjectHandler>().As<IHandler<object>>();
                var container = builder.Build();
                Assert.False(container.IsRegistered<IHandler<AEnum>>());
            }
        }
    }
}
