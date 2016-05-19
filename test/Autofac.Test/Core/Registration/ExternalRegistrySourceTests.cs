using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public class ExternalRegistrySourceTests
    {
        public interface IServiceA
        {
        }

        public interface IServiceB
        {
        }

        public class ClassA : IServiceA, IServiceB
        {
        }

        public class ClassB : IServiceA
        {
        }

        [Fact]
        public void OneTypeImplementTwoInterfaces_OtherObjectsImplementingOneOfThoseInterfaces_CanBeResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ClassA)).As(typeof(IServiceA), typeof(IServiceB));
            builder.RegisterType(typeof(ClassB)).As(typeof(IServiceA));

            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            lifetime.Resolve<IServiceB>();

            var allImplementationsOfServiceA = lifetime.Resolve<IEnumerable<IServiceA>>();
            Assert.Equal(2, allImplementationsOfServiceA.Count());
        }
    }
}
