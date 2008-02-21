using System;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Registrars
{
    [TestFixture]
    public class ConcreteRegistrarFixture
    {
        interface IA { }
        interface IB { }
        interface IC { }

        class Abc : DisposeTracker, IA, IB, IC { }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterTypeAsUnsupportedService()
        {
            new ContainerBuilder().Register<object>().As<string>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            new ContainerBuilder().Register<object>().As<object, string>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterInstanceAsUnsupportedService()
        {
            new ContainerBuilder().Register(new object()).As<string>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterDelegateAsUnsupportedService()
        {
            new ContainerBuilder().Register(c => new object()).As<string>();
        }

        [Test]
        public void RegisterThreeServices()
        {
            var target = new ContainerBuilder();
            target.Register<Abc>()
                .As<IA, IB, IC>()
                .WithScope(InstanceScope.Singleton);
            var container = target.Build();
            var a = container.Resolve<IA>();
            var b = container.Resolve<IB>();
            var c = container.Resolve<IC>();
            Assert.IsNotNull(a);
            Assert.AreSame(a, b);
            Assert.AreSame(b, c);
        }
    }
}
