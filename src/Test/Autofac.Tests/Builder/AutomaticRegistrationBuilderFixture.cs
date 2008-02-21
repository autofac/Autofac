using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class AutomaticRegistrationBuilderFixture
    {
        [Test]
        public void AutomaticRegistrationBasedOnPredicate()
        {
            var builder = new ContainerBuilder();
            builder.RegisterTypesMatching(t => t == typeof(object));
            var container = builder.Build();
            Assert.IsTrue(container.IsRegistered<object>());
            Assert.IsNotNull(container.Resolve<object>());
            Assert.IsFalse(container.IsRegistered<string>());
        }

        [Test]
        public void NoAutomaticRegistrationOnFalsePredicate()
        {
            var builder = new ContainerBuilder();
            builder.RegisterTypesMatching(t => false);
            var container = builder.Build();
            Assert.IsFalse(container.IsRegistered<object>());
        }

        [Test]
        public void AutomaticRegistrationFromAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterTypesFromAssembly(GetType().Assembly);
            var container = builder.Build();
            Assert.IsTrue(container.IsRegistered(GetType()));
            Assert.IsFalse(container.IsRegistered<string>());
        }

        interface IController { }
        class AController : IController { }
        class NotController { }

        [Test]
        public void AutomaticRegistrationOfAssignable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterTypesAssignableTo<IController>();
            var container = builder.Build();
            Assert.IsTrue(container.IsRegistered<AController>());
            Assert.IsFalse(container.IsRegistered<NotController>());
        }

        abstract class AbstractController : IController { }
        
        [Test]
        public void DoesntRegisterAbstractClasses()
        {
            var builder = new ContainerBuilder();
            builder.RegisterTypesAssignableTo<IController>();
            var container = builder.Build();
            Assert.IsFalse(container.IsRegistered<AbstractController>());
        }
    }
}
