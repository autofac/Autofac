using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class ExternalRegistrySourceTests
    {
        interface IServiceA { }
        interface IServiceB { }
        class ClassA : IServiceA, IServiceB { }
        class ClassB : IServiceA { }

        // Courtesy of M. Kowalewski
        [Test]
        public void OneTypeImplementTwoInterfaces_OtherObjectsImplementingOneOfThoseInterfaces_CanBeResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ClassA)).As(typeof(IServiceA), typeof(IServiceB));
            builder.RegisterType(typeof(ClassB)).As(typeof(IServiceA));

            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            lifetime.Resolve<IServiceB>();

            var allImplementationsOfServiceA = lifetime.Resolve<IEnumerable<IServiceA>>();
            Assert.AreEqual(2, allImplementationsOfServiceA.Count());
        }
    }
}
