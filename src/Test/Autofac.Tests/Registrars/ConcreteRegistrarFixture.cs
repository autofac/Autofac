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
            new ContainerBuilder().Register<string>().As<IA>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            new ContainerBuilder().Register<string>().As<IA, IB>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterInstanceAsUnsupportedService()
        {
            new ContainerBuilder().Register("hello").As<IA>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterDelegateAsUnsupportedService()
        {
            new ContainerBuilder().Register(c => "hello").As<IA>();
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

        [Test]
        public void OnlyIfNotRegisteredFiltersServices()
        {
            var builder = new ContainerBuilder();
            builder.Register("s1");
            builder.Register("s2").Named("name").DefaultOnly();
            var container = builder.Build();
            Assert.AreEqual("s1", container.Resolve<string>()); // Not overridden
            Assert.AreEqual("s2", container.Resolve("name"));
        }
    }
}
