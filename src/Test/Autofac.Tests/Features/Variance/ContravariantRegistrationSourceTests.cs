using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;

namespace Autofac.Tests.Features.Variance
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

    static class HandlerTestExtensions
    {
        public static void AssertSingleHandlerCanHandle<TCommand>(this IContainer container)
        {
            var handlers = container.ComponentRegistry.RegistrationsFor(new TypedService(typeof(IHandler<TCommand>)));
            Assert.AreEqual(1, handlers.Count());
            container.Resolve<IHandler<TCommand>>();
        }
    }

    public class ContravariantRegistrationSourceTests
    {
        [TestFixture]
        public class WhenAHandlerForAConcreteTypeIsRegistered
        {
            IContainer _container;

            [SetUp]
            public void SetUp()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<BHandler>().As<IHandler<CommandB>>();
                _container = builder.Build();
            }

            [Test]
            public void TheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandB>();
            }

            [Test]
            public void DirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandC>();
            }

            [Test]
            public void IndirectSubclassesOfTheCommandTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandD>();
            }

            [Test]
            public void BaseClassesOfTheCommandCannotBeHandled()
            {
                Assert.IsFalse(_container.IsRegistered<IHandler<CommandA>>());
            }

            [Test]
            public void UnrelatedCommandsCannotBeHandled()
            {
                Assert.IsFalse(_container.IsRegistered<IHandler<UnrelatedCommand>>());
            }
        }

        [TestFixture]
        public class WhenAHandlerForAnInterfaceTypeIsRegistered
        {
            IContainer _container;

            [SetUp]
            public void SetUp()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<InterfaceHandler>().As<IHandler<ICommand>>();
                _container = builder.Build();
            }

            [Test]
            public void TheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<ICommand>();
            }

            [Test]
            public void ImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandB>();
            }

            [Test]
            public void IndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandC>();
            }

            [Test]
            public void SecondOrderIndirectImplementersOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandD>();
            }

            [Test]
            public void DerivationsOfTheInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<IDerivedCommand>();
            }

            [Test]
            public void UnrelatedCommandsCannotBeHandled()
            {
                Assert.IsFalse(_container.IsRegistered<IHandler<UnrelatedCommand>>());
            }

            [Test]
            public void BaseInterfacesOfTheCommandCannotBeHandled()
            {
                Assert.IsFalse(_container.IsRegistered<IHandler<IBaseCommand>>());
            }
        }

        [TestFixture]
        public class WhenAHandlerForObjectIsRegistered
        {
            IContainer _container;

            [SetUp]
            public void SetUp()
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterType<ObjectHandler>().As<IHandler<object>>();
                _container = builder.Build();
            }

            [Test]
            public void ObjectCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<object>();
            }

            [Test]
            public void AnyConcreteTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<CommandA>();
            }

            [Test]
            public void AnyInterfaceTypeCanBeHandled()
            {
                _container.AssertSingleHandlerCanHandle<ICommand>();
            }
        }
    }
}
